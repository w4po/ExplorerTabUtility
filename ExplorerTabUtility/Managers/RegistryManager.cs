using System;
using Microsoft.Win32;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Managers;

public static class RegistryManager
{
    public static bool IsInStartup()
    {
        var executablePath = Helper.GetExecutablePath();
        if (string.IsNullOrWhiteSpace(executablePath)) return false;

        using var key = OpenCurrentUserKey(Constants.RunRegistryKeyPath, false);
        if (key == null) return false;

        var value = key.GetValue(Constants.AppName) as string;
        return string.Equals(value, executablePath, StringComparison.OrdinalIgnoreCase);
    }
    
    public static void ToggleStartup()
    {
        var executablePath = Helper.GetExecutablePath();
        if (string.IsNullOrWhiteSpace(executablePath)) return;

        using var key = OpenCurrentUserKey(Constants.RunRegistryKeyPath, true);
        if (key == null) return;

        // If already set in startup
        if (string.Equals(key.GetValue(Constants.AppName) as string, executablePath, StringComparison.OrdinalIgnoreCase))
        {
            // Remove from startup
            key.DeleteValue(Constants.AppName, false);
        }
        else
        {
            // Add to startup
            key.SetValue(Constants.AppName, executablePath);
        }
    }
    
    private static RegistryKey? OpenCurrentUserKey(string name, bool writable) => Registry.CurrentUser.OpenSubKey(name, writable);
}