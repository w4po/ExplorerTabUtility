using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;

namespace ExplorerTabUtility.Forms;

public class TrayIcon : ApplicationContext
{
    private static NotifyIcon _notifyIcon = default!;
    private static KeyboardHook _keyboardHook = default!;
    private static UiAutomation _uiAutomation = default!;
    private static readonly SemaphoreSlim Limiter = new(1);
    private static IntPtr _mainWindowHandle = IntPtr.Zero;

    public TrayIcon()
    {
        _keyboardHook = new KeyboardHook(OnNewWindow);
        _uiAutomation = new UiAutomation(OnNewWindow);

        InitializeComponent();

        Application.ApplicationExit += OnApplicationExit;
    }

    private static void InitializeComponent()
    {
        var windowHMenuItem = new ToolStripMenuItem("All Windows");
        var keyboardHMenuItem = new ToolStripMenuItem("Keyboard (Win + E)");
        var startupMenuItem = new ToolStripMenuItem("Add to startup");
        var exitMenuItem = new ToolStripMenuItem("Exit");

        windowHMenuItem.Checked = Properties.Settings.Default.WindowHook;
        keyboardHMenuItem.Checked = Properties.Settings.Default.KeyboardHook;
        startupMenuItem.Checked = IsInStartup();

        if (windowHMenuItem.Checked)
            _uiAutomation.StartHook();

        if (keyboardHMenuItem.Checked)
            _keyboardHook.StartHook();

        _notifyIcon = new NotifyIcon
        {
            Icon = Helper.GetIcon(),
            Text = "Explorer Tab Utility: Force new windows to tabs.",

            ContextMenuStrip = new ContextMenuStrip()
        };

        _notifyIcon.ContextMenuStrip.Items.Add(windowHMenuItem);
        _notifyIcon.ContextMenuStrip.Items.Add(keyboardHMenuItem);
        _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _notifyIcon.ContextMenuStrip.Items.Add(startupMenuItem);
        _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _notifyIcon.ContextMenuStrip.Items.Add(exitMenuItem);

        windowHMenuItem.Click += (_, _) => ToggleWindowHook(windowHMenuItem);
        keyboardHMenuItem.Click += (_, _) => ToggleKeyboardHook(keyboardHMenuItem);
        startupMenuItem.Click += (_, _) => ToggleStartup(startupMenuItem);
        exitMenuItem.Click += (_, _) => Application.Exit();

        _notifyIcon.Visible = true;
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
    private static void ToggleStartup(ToolStripMenuItem startupMenuItem)
    {
        var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(executablePath)) return;

        using var key = Registry.CurrentUser.OpenSubKey(Constants.RunRegistryKeyPath, true);
        if (key == default) return;

        // If already set in startup
        if (string.Equals(key.GetValue(Constants.AppName) as string, executablePath, StringComparison.OrdinalIgnoreCase))
        {
            // Remove from startup
            key.DeleteValue(Constants.AppName, false);
            startupMenuItem.Checked = false;
        }
        else
        {
            // Add to startup
            key.SetValue(Constants.AppName, executablePath);
            startupMenuItem.Checked = true;
        }
    }
    private static void ToggleWindowHook(ToolStripMenuItem windowHMenuItem)
    {
        windowHMenuItem.Checked = !windowHMenuItem.Checked;

        Properties.Settings.Default.WindowHook = windowHMenuItem.Checked;
        Properties.Settings.Default.Save();

        if (windowHMenuItem.Checked)
            _uiAutomation.StartHook();
        else
            _uiAutomation.StopHook();
    }
    private static void ToggleKeyboardHook(ToolStripMenuItem keyboardHMenuItem)
    {
        keyboardHMenuItem.Checked = !keyboardHMenuItem.Checked;

        Properties.Settings.Default.KeyboardHook = keyboardHMenuItem.Checked;
        Properties.Settings.Default.Save();

        if (keyboardHMenuItem.Checked)
            _keyboardHook.StartHook();
        else
            _keyboardHook.StopHook();
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

            var oldTabs = WinApi.GetAllExplorerTabs();

            UiAutomation.AddNewTab(windowElement);

            // If it is just a new (This PC) window, return.
            if (string.IsNullOrWhiteSpace(window.Path)) return;
            if (!Uri.TryCreate(window.Path, UriKind.Absolute, out var uri)) return;

            var newTabHandle = WinApi.ListenForNewExplorerTab(oldTabs);
            if (newTabHandle == default) return;

            var newTabElement = UiAutomation.FromHandle(newTabHandle);
            if (newTabElement == default) return;

            UiAutomation.GoToLocation(uri.LocalPath, windowElement);

            if (window.SelectedItems is not { } selectedItems) return;

            UiAutomation.SelectItems(newTabElement, selectedItems);
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
        if (WinApi.IsWindowStillHasClassName(_mainWindowHandle, "CabinetWClass"))
            return _mainWindowHandle;

        var allWindows = WinApi.FindAllWindowsEx();

        // Get another handle other than the newly created one. (In case if it still alive.)
        _mainWindowHandle = allWindows.FirstOrDefault(t => t != otherThan);

        return _mainWindowHandle;
    }

    private static void OnApplicationExit(object? _, EventArgs __)
    {
        _notifyIcon.Visible = false;
        _keyboardHook.Dispose();
        _uiAutomation.Dispose();
    }
}