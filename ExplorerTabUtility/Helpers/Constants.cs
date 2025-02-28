namespace ExplorerTabUtility.Helpers;

internal class Constants
{
    internal const string AppName = "ExplorerTabUtility";
    internal const string MutexId = $"__{AppName}Hook__Mutex";
    internal const string RunRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    internal const string ExplorerAdvancedKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    internal const string NotifyIconText = @"Explorer Tab Utility: Force new windows to tabs.";
    internal const string HotKeyProfilesFileName = "HotKeyProfiles.json";
    internal const string JsonFileFilter = @"JSON files (*.json)|*.json|All Files|*.*";
    internal const string UpdateUrl = "https://api.github.com/repos/w4po/ExplorerTabUtility/releases/latest";
}