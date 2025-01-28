namespace ExplorerTabUtility.Managers;

public static class SettingsManager
{
    public static bool IsKeyboardHookActive
    {
        get => Properties.Settings.Default.KeyboardHook;
        set
        {
            Properties.Settings.Default.KeyboardHook = value;
            SaveSettings();
        }
    }
    public static bool IsWindowHookActive
    {
        get => Properties.Settings.Default.WindowHook;
        set
        {
            Properties.Settings.Default.WindowHook = value;
            SaveSettings();
        }
    }
    public static string HotKeyProfiles
    {
        get => Properties.Settings.Default.HotKeyProfiles;
        set
        {
            Properties.Settings.Default.HotKeyProfiles = value;
            SaveSettings();
        }
    }
    public static bool SaveProfilesOnExit
    {
        get => Properties.Settings.Default.SaveProfilesOnExit;
        set
        {
            Properties.Settings.Default.SaveProfilesOnExit = value;
            SaveSettings();
        }
    }
    
    public static void SaveSettings() => Properties.Settings.Default.Save();
}
