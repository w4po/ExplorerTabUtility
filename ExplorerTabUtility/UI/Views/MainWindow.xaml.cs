using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;

namespace ExplorerTabUtility.UI.Views;

// ReSharper disable once RedundantExtendsListEntry
public partial class MainWindow : Window
{
    private readonly HookManager _hookManager;
    private readonly ProfileManager _profileManager;
    private readonly SystemTrayIcon _notifyIconManager;
    private nint _handle;

    public MainWindow()
    {
        InitializeComponent();

        // Set initial size from settings
        Width = SettingsManager.FormSize.Width;
        Height = SettingsManager.FormSize.Height;

        _profileManager = new ProfileManager(ProfilesPanel);
        _hookManager = new HookManager(_profileManager);
        _notifyIconManager = new SystemTrayIcon(_profileManager, _hookManager, ShowWindow);

        SetupEventHandlers();
        StartHooks();

        CbAutoUpdate.IsChecked = SettingsManager.AutoUpdate;
        CbThemeIssue.IsChecked = SettingsManager.HaveThemeIssue;
        CbHideTrayIcon.IsChecked = SettingsManager.IsTrayIconHidden;
        CbAutoSaveProfiles.IsChecked = SettingsManager.SaveProfilesOnExit;
        CbSaveClosedWindows.IsChecked = SettingsManager.SaveClosedWindows;
        UpdateTrayIconVisibility(false);

        if (SettingsManager.AutoUpdate)
            UpdateManager.CheckForUpdates();

        // Show the window if this is the first run
        if (SettingsManager.IsFirstRun)
        {
            SettingsManager.IsFirstRun = false;
            Show();
        }
    }

    private void SetupEventHandlers()
    {
        Application.Current.Exit += OnApplicationExit;
        _hookManager.OnVisibilityToggled += ToggleWindowVisibility;

        BtnNewProfile.Click += BtnNewProfile_Click;
        BtnImport.Click += BtnImport_Click;
        BtnExport.Click += BtnExport_Click;
        BtnSave.Click += BtnSave_Click;
        CbAutoSaveProfiles.Checked += CbAutoSaveProfiles_CheckedChanged;
        CbAutoSaveProfiles.Unchecked += CbAutoSaveProfiles_CheckedChanged;
        CbSaveClosedWindows.Checked += CbSaveClosedWindows_CheckedChanged;
        CbSaveClosedWindows.Unchecked += CbSaveClosedWindows_CheckedChanged;
        CbAutoUpdate.Checked += CbAutoUpdate_CheckedChanged;
        CbAutoUpdate.Unchecked += CbAutoUpdate_CheckedChanged;
        CbThemeIssue.Checked += CbThemeIssue_CheckedChanged;
        CbThemeIssue.Unchecked += CbThemeIssue_CheckedChanged;
        CbHideTrayIcon.Checked += CbHideTrayIcon_CheckedChanged;
        CbHideTrayIcon.Unchecked += CbHideTrayIcon_CheckedChanged;

        // Window events
        SizeChanged += MainWindow_SizeChanged;
        Closing += MainWindow_Closing;
        Deactivated += MainWindow_Deactivated;

        // Custom title bar event handlers
        TitleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
        MinimizeButton.Click += MinimizeButton_Click;
        MaximizeButton.Click += MaximizeButton_Click;
        CloseButton.Click += CloseButton_Click;
    }

    private void StartHooks()
    {
        if (SettingsManager.IsWindowHookActive) _hookManager.StartWindowHook();
        if (SettingsManager.IsMouseHookActive) _hookManager.StartMouseHook();
        if (SettingsManager.IsKeyboardHookActive) _hookManager.StartKeyboardHook();
        _hookManager.SetReuseTabs(SettingsManager.ReuseTabs);
    }

    private void ToggleWindowVisibility()
    {
        if (Visibility == Visibility.Visible)
            HideWindow();
        else
            ShowWindow();
    }

    private void ShowWindow()
    {
        Dispatcher.Invoke(Show);
        WinApi.RestoreWindowToForeground(_handle);
    }

    private void HideWindow(bool exit = false)
    {
        Dispatcher.BeginInvoke(Hide);

        if (CbAutoSaveProfiles.IsChecked != true) return;

        _profileManager.SaveProfiles();
        _notifyIconManager.UpdateMenuItems(autoCheckParent: !exit);
        UpdateTrayIconVisibility(false);
    }

    private void BtnNewProfile_Click(object? _, RoutedEventArgs __) => _profileManager.AddProfile();

    private void BtnImport_Click(object? _, RoutedEventArgs __)
    {
        var ofd = new OpenFileDialog
        {
            FileName = Constants.HotKeyProfilesFileName,
            Filter = Constants.JsonFileFilter
        };

        if (ofd.ShowDialog() != true) return;
        var jsonString = System.IO.File.ReadAllText(ofd.FileName);
        _profileManager.ImportProfiles(jsonString);
        _notifyIconManager.UpdateMenuItems();
        UpdateTrayIconVisibility(false);
    }

    private void BtnExport_Click(object? _, RoutedEventArgs __)
    {
        var sfd = new SaveFileDialog
        {
            FileName = Constants.HotKeyProfilesFileName,
            Filter = Constants.JsonFileFilter
        };

        if (sfd.ShowDialog() != true) return;
        using var openFile = sfd.OpenFile();
        var jsonString = _profileManager.ExportProfiles();
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        openFile.Write(bytes, 0, bytes.Length);
    }

    private void BtnSave_Click(object? _, RoutedEventArgs __)
    {
        _profileManager.SaveProfiles();
        _notifyIconManager.UpdateMenuItems();
        UpdateTrayIconVisibility(false);
    }

    private void CbAutoSaveProfiles_CheckedChanged(object? _, RoutedEventArgs __)
    {
        SettingsManager.SaveProfilesOnExit = CbAutoSaveProfiles.IsChecked ?? false;
    }

    private void CbAutoUpdate_CheckedChanged(object? _, RoutedEventArgs __)
    {
        SettingsManager.AutoUpdate = CbAutoUpdate.IsChecked ?? false;
    }

    private void CbThemeIssue_CheckedChanged(object? _, RoutedEventArgs __)
    {
        SettingsManager.HaveThemeIssue = CbThemeIssue.IsChecked ?? false;
    }

    private void CbSaveClosedWindows_CheckedChanged(object? _, RoutedEventArgs __)
    {
        SettingsManager.SaveClosedWindows = CbSaveClosedWindows.IsChecked ?? false;
    }

    private void CbHideTrayIcon_CheckedChanged(object? _, RoutedEventArgs __) => UpdateTrayIconVisibility(true);

    private void UpdateTrayIconVisibility(bool showAlert)
    {
        // Check for valid toggle visibility profile
        var profile = _profileManager
            .GetProfiles()
            .FirstOrDefault(p =>
                p is { IsEnabled: true, Action: HotKeyAction.ToggleVisibility } &&
                (p.IsMouse ? SettingsManager.IsMouseHookActive : SettingsManager.IsKeyboardHookActive));

        var canToggleVisibility = profile != null;
        var isChecked = CbHideTrayIcon.IsChecked == true;
        if (isChecked && showAlert && !SettingsManager.IsTrayIconHidden)
        {
            var message = canToggleVisibility
                ? $"You can show the app again by pressing {profile!.HotKeys!.HotKeysToString(profile.IsDoubleClick)}"
                : "Cannot hide tray icon if no hotkey is configured to toggle visibility.";

            CustomMessageBox.Show(this, message, Constants.AppName);
        }

        var newCheckedState = canToggleVisibility && isChecked;
        CbHideTrayIcon.IsChecked = newCheckedState;
        SettingsManager.IsTrayIconHidden = newCheckedState;
        _notifyIconManager.SetTrayIconVisibility(visible: !newCheckedState);
    }

    private void MainWindow_Deactivated(object? _, EventArgs __)
    {
        // Find the focused HotKeyProfileControl, if any
        foreach (var child in ProfilesPanel.Children)
        {
            if (child is not HotKeyProfileControl control) continue;
            if (!control.TxtHotKeys.IsFocused) continue;

            control.TxtName.Focus();
            break;
        }
    }

    private void MainWindow_SizeChanged(object? _, SizeChangedEventArgs __)
    {
        if (WindowState == WindowState.Normal)
            SettingsManager.FormSize = new Size(Width, Height);
    }

    private void MainWindow_Closing(object? _, System.ComponentModel.CancelEventArgs e)
    {
        // ALT + F4 / App exit
        e.Cancel = true;
        HideWindow(exit: true);
    }

    private void OnApplicationExit(object _, ExitEventArgs __)
    {
        _notifyIconManager.Dispose();
        _hookManager.Dispose();
    }

    private void TitleBar_MouseLeftButtonDown(object _, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
            return;

        if (e.ClickCount == 2)
        {
            MaximizeRestore();
            return;
        }

        // Handle dragging
        if (WindowState != WindowState.Maximized)
        {
            DragMove();
            return;
        }

        WinApi.SendMessage(_handle, 0xA1, 2, 0);
    }

    private void MaximizeButton_Click(object? _, RoutedEventArgs __) => MaximizeRestore();
    private void MinimizeButton_Click(object? _, RoutedEventArgs __) => HideWindow();
    private void CloseButton_Click(object? _, RoutedEventArgs __) => HideWindow();

    private void MaximizeRestore()
    {
        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _handle = new WindowInteropHelper(this).Handle;
    }
}