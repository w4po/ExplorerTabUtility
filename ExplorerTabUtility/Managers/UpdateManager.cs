using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices;
using AutoUpdaterDotNET;
using AutoUpdaterDotNET.Markdown;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Managers;

internal static class UpdateManager
{
    static UpdateManager()
    {
        AutoUpdater.FlattenRootFolder = true;
        AutoUpdater.Icon = Helper.GetIcon()!.ToBitmap();
        AutoUpdater.ChangelogViewerProvider = new MarkdownViewerProvider();

        AutoUpdater.ParseUpdateInfoEvent += ParseUpdateInfo;
    }

    public static void CheckForUpdates() => AutoUpdater.Start(Constants.UpdateUrl);

    private static void ParseUpdateInfo(ParseUpdateInfoEventArgs p)
    {
        try
        {
            var jsonNode = JsonSerializer.Deserialize<JsonNode>(p.RemoteData);
            if (jsonNode == null) return;

            p.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = jsonNode["tag_name"]!.GetValue<string>().TrimStart('v'),
                ChangelogText = jsonNode["body"]!.GetValue<string>(),
                ChangelogURL = jsonNode["html_url"]!.GetValue<string>(),
                DownloadURL = FindMatchingUpdateAssetUrl(jsonNode)
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update check failed: {ex.Message}");
        }
    }

    private static string? FindMatchingUpdateAssetUrl(JsonNode jsonNode)
    {
        var currentRuntime = Environment.Version.Major > 5 ? $"Net{Environment.Version.Major}" : "NetFW";
        var currentArch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            _ => "x64"
        };

        var assets = jsonNode["assets"]!.AsArray();
        foreach (var asset in assets)
        {
            var assetName = asset!["name"]!.GetValue<string>();
            var split = assetName.Split('_');
            if (split.Length < 4)
                continue;

            var arch = split[2].Split('.')[0];
            var runtime = split[3].Replace(".zip", "");

            if (runtime.StartsWith(currentRuntime) && arch == currentArch)
                return asset["browser_download_url"]!.GetValue<string>();
        }

        return null;
    }
}