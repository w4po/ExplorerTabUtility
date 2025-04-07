namespace ExplorerTabUtility.Helpers;

internal static class Constants
{
    internal const string AppName = "ExplorerTabUtility";
    internal const string MutexId = $"__{AppName}Hook__Mutex";
    internal const string NotifyIconText = "Explorer Tab Utility: Force new windows to tabs.";
    internal const string SettingsFileName = "settings.json";
    internal const string HotKeyProfilesFileName = "HotKeyProfiles.json";
    internal const string JsonFileFilter = "JSON files (*.json)|*.json|All Files|*.*";
    internal const string UpdateUrl = "https://api.github.com/repos/w4po/ExplorerTabUtility/releases/latest";
    internal const string DefaultHotKeyProfiles = "[{\"Name\":\"Home\",\"HotKeys\":[91,69],\"Scope\":0,\"Action\":0,\"Path\":\"\",\"IsHandled\":true,\"IsEnabled\":true,\"Delay\":0},{\"Name\":\"Duplicate\",\"HotKeys\":[17,68],\"Scope\":1,\"Action\":1,\"Path\":null,\"IsHandled\":true,\"IsEnabled\":true,\"Delay\":0},{\"Name\":\"ReopenClosed\",\"HotKeys\":[16,17,84],\"Scope\":1,\"Action\":2,\"Path\":null,\"IsHandled\":true,\"IsEnabled\":true,\"Delay\":0}]";
}