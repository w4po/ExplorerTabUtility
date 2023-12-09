using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using FlaUI.Core.AutomationElements;
using Microsoft.Win32;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.Helpers;
using Window = ExplorerTabUtility.Models.Window;

namespace ExplorerTabUtility.Forms;

public class TrayIcon : ApplicationContext
{
    private static IntPtr _mainWindowHandle = IntPtr.Zero;
    private static readonly NotifyIcon NotifyIcon;
    private static readonly Keyboard KeyboardHook;
    private static readonly Shell32 WindowHook;
    private static readonly SemaphoreSlim Limiter;
    private static WindowHookVia _windowHookVia;

    static TrayIcon()
    {
        Limiter = new SemaphoreSlim(1);
        WindowHook = new Shell32(OnNewWindow);
        KeyboardHook = new Keyboard(OnNewWindow);

        var isKeyboardHookActive = Properties.Settings.Default.KeyboardHook;
        var isWindowHookActive = Properties.Settings.Default.WindowHook;
        _windowHookVia = Properties.Settings.Default.WindowViaUi
            ? WindowHookVia.Ui
            : WindowHookVia.Keys;

        NotifyIcon = new NotifyIcon
        {
            Icon = Helper.GetIcon(),
            Text = "Explorer Tab Utility: Force new windows to tabs.",

            ContextMenuStrip = CreateContextMenuStrip(isKeyboardHookActive, isWindowHookActive),
            Visible = true
        };

        if (isKeyboardHookActive) KeyboardHook.StartHook();
        if (isWindowHookActive) WindowHook.StartHook();

        Application.ApplicationExit += OnApplicationExit;
    }
    private static ContextMenuStrip CreateContextMenuStrip(bool isKeyboardHookActive, bool isWindowHookActive)
    {
        var strip = new ContextMenuStrip();

        strip.Items.Add(CreateToolStripMenuItem("Keyboard (Win + E)", isKeyboardHookActive, ToggleKeyboardHook));
        strip.Items.Add(CreateWindowHookMenuItem(isWindowHookActive));

        strip.Items.Add(new ToolStripSeparator());
        strip.Items.Add(CreateToolStripMenuItem("Add to startup", IsInStartup(), ToggleStartup));

        strip.Items.Add(new ToolStripSeparator());
        strip.Items.Add(CreateToolStripMenuItem("Exit", false, static (_, _) => Application.Exit()));

        return strip;
    }
    private static ToolStripMenuItem CreateWindowHookMenuItem(bool isWindowHookActive)
    {
        var windowHookMenuItem = CreateToolStripMenuItem("All Windows", isWindowHookActive, ToggleWindowHook);

        windowHookMenuItem.DropDownItems.Add(
            CreateToolStripMenuItem("UI (Recommended)", _windowHookVia == WindowHookVia.Ui, WindowHookViaChanged, "WindowViaUi"));

        windowHookMenuItem.DropDownItems.Add(
            CreateToolStripMenuItem("Keys", _windowHookVia == WindowHookVia.Keys, WindowHookViaChanged, "WindowViaKeys"));

        return windowHookMenuItem;
    }
    private static ToolStripMenuItem CreateToolStripMenuItem(string text, bool isChecked, EventHandler eventHandler, string? name = default)
    {
        var item = new ToolStripMenuItem
        {
            Text = text,
            Checked = isChecked
        };

        if (name != default)
            item.Name = name;

        item.Click += eventHandler;
        return item;
    }

    private static bool IsInStartup()
    {
        var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(executablePath)) return false;

        using var key = Registry.CurrentUser.OpenSubKey(Constants.RunRegistryKeyPath, false);
        if (key == default) return false;

        var value = key.GetValue(Constants.AppName) as string;
        return string.Equals(value, executablePath, StringComparison.OrdinalIgnoreCase);
    }
    private static void ToggleStartup(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(executablePath)) return;

        using var key = Registry.CurrentUser.OpenSubKey(Constants.RunRegistryKeyPath, true);
        if (key == default) return;

        // If already set in startup
        if (string.Equals(key.GetValue(Constants.AppName) as string, executablePath, StringComparison.OrdinalIgnoreCase))
        {
            // Remove from startup
            key.DeleteValue(Constants.AppName, false);
            item.Checked = false;
        }
        else
        {
            // Add to startup
            key.SetValue(Constants.AppName, executablePath);
            item.Checked = true;
        }
    }
    private static void ToggleKeyboardHook(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        item.Checked = !item.Checked;

        Properties.Settings.Default.KeyboardHook = item.Checked;
        Properties.Settings.Default.Save();

        if (item.Checked)
            KeyboardHook.StartHook();
        else
            KeyboardHook.StopHook();
    }
    private static void ToggleWindowHook(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        item.Checked = !item.Checked;

        Properties.Settings.Default.WindowHook = item.Checked;
        Properties.Settings.Default.Save();

        if (item.Checked)
            WindowHook.StartHook();
        else
            WindowHook.StopHook();

        foreach (ToolStripItem subItem in item.DropDownItems)
            subItem.Enabled = item.Checked;
    }
    private static void WindowHookViaChanged(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        var container = item.GetCurrentParent();
        foreach (ToolStripMenuItem radio in container.Items)
        {
            radio.Checked = !radio.Checked;

            if (radio.Name == "WindowViaUi")
                Properties.Settings.Default.WindowViaUi = radio.Checked;
            else if (radio.Name == "WindowViaKeys")
                Properties.Settings.Default.WindowViaKeys = radio.Checked;
        }

        Properties.Settings.Default.Save();

        _windowHookVia = Properties.Settings.Default.WindowViaUi
            ? WindowHookVia.Ui
            : WindowHookVia.Keys;
    }

    private static async Task OnNewWindow(Window window)
    {
        var windowHandle = IntPtr.Zero;
        try
        {
            await Limiter.WaitAsync().ConfigureAwait(false);

            windowHandle = GetMainWindowHWnd(window.OldWindowHandle);
            if (windowHandle == default) return;

            var windowElement = UiAutomation.FromHandle(windowHandle);
            if (windowElement == default) return;

            // Store currently opened Tabs, before we open a new one.
            var oldTabs = WinApi.GetAllExplorerTabs();

            // Add new tab.
            AddNewTab(windowElement);

            // If it is just a new (This PC | Home), return.
            if (string.IsNullOrWhiteSpace(window.Path)) return;

            // Get newly created tab's handle (That is not in 'oldTabs')
            var newTabHandle = WinApi.ListenForNewExplorerTab(oldTabs);
            if (newTabHandle == 0) return;

            // Get the tab element out of that handle.
            var newTabElement = UiAutomation.FromHandle(newTabHandle);
            if (newTabElement == default) return;

            // Navigate to the target location
            Navigate(windowElement, newTabHandle, window.Path);

            if (window.SelectedItems is not { Count: > 0 } selectedItems) return;

            // Select items
            SelectItems(newTabElement, selectedItems);
        }
        finally
        {
            if (windowHandle != default)
                WinApi.RestoreWindowToForeground(windowHandle);

            Limiter.Release();
        }
    }

    private static IntPtr GetMainWindowHWnd(IntPtr otherThan)
    {
        if (WinApi.IsWindowHasClassName(_mainWindowHandle, "CabinetWClass"))
            return _mainWindowHandle;

        var allWindows = WinApi.FindAllWindowsEx();

        // Get another handle other than the newly created one. (In case if it still alive.)
        _mainWindowHandle = allWindows.FirstOrDefault(t => t != otherThan);

        return _mainWindowHandle;
    }
    private static void AddNewTab(AutomationElement window)
    {
        // if via UI is selected try to add a new tab with UI Automation.
        if (_windowHookVia == WindowHookVia.Ui && UiAutomation.AddNewTab(window))
            return;

        // Via Keys is selected or UI Automation fails.
        Keyboard.AddNewTab(window.Properties.NativeWindowHandle.Value);
    }
    private static void Navigate(AutomationElement window, nint tabHandle, string location)
    {
        // if via UI is selected try to Navigate with UI Automation.
        if (_windowHookVia == WindowHookVia.Ui && UiAutomation.Navigate(window, tabHandle, location))
            return;

        // Via Keys is selected or UI Automation fails.
        Keyboard.Navigate(window.Properties.NativeWindowHandle.Value, tabHandle, location);
    }
    private static void SelectItems(AutomationElement tab, ICollection<string> names)
    {
        // if via UI is selected try to Select with UI Automation.
        if (_windowHookVia == WindowHookVia.Ui && UiAutomation.SelectItems(tab, names))
            return;

        // Via Keys is selected or UI Automation fails.
        Keyboard.SelectItems(tab.Properties.NativeWindowHandle.Value, names);
    }
    private static void OnApplicationExit(object? _, EventArgs __)
    {
        NotifyIcon.Visible = false;
        KeyboardHook.Dispose();
        WindowHook.Dispose();
    }
}