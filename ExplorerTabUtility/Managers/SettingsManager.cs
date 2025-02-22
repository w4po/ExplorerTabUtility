using System.Drawing;

namespace ExplorerTabUtility.Managers;

public static class SettingsManager
{
    public static bool IsMouseHookActive
    {
        get => Properties.Settings.Default.MouseHook;
        set
        {
            Properties.Settings.Default.MouseHook = value;
            SaveSettings();
        }
    }
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
    public static bool ReuseTabs
    {
        get => Properties.Settings.Default.ReuseTabs;
        set
        {
            Properties.Settings.Default.ReuseTabs = value;
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
    public static Size FormSize
    {
        get => Properties.Settings.Default.FormSize;
        set
        {
            Properties.Settings.Default.FormSize = value;
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
    public static bool IsFirstRun
    {
        get => Properties.Settings.Default.IsFirstRun;
        set
        {
            Properties.Settings.Default.IsFirstRun = value;
            SaveSettings();
        }
    }
    public static bool IsTrayIconHidden
    {
        get => Properties.Settings.Default.IsTrayIconHidden;
        set
        {
            Properties.Settings.Default.IsTrayIconHidden = value;
            SaveSettings();
        }
    }
    public static bool HaveThemeIssue
    {
        get => Properties.Settings.Default.HaveThemeIssue;
        set
        {
            Properties.Settings.Default.HaveThemeIssue = value;
            SaveSettings();
        }
    }
    
    public static void SaveSettings() => Properties.Settings.Default.Save();
}
