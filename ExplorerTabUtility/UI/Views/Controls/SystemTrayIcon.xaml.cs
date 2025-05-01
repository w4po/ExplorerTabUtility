using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.UI.Commands;

namespace ExplorerTabUtility.UI.Views.Controls;

// ReSharper disable once RedundantExtendsListEntry
public partial class SystemTrayIcon : UserControl, IDisposable
{
    private readonly ProfileManager _profileManager;
    private readonly HookManager _hookManager;
    private readonly Action _showWindowAction;
    private ICommand ProfileItemCommand { get; set; } = null!;
    private bool _savedReuseTabsState;

    public SystemTrayIcon(ProfileManager profileManager, HookManager hookManager, Action showWindowAction)
    {
        InitializeComponent();
        InitializeCommands();

        TrayIcon.Icon = Helper.GetIcon();
        TrayIcon.ToolTipText = Constants.NotifyIconText;

        _profileManager = profileManager;
        _hookManager = hookManager;
        _showWindowAction = showWindowAction;

        _hookManager.OnShellInitialized += HookManager_OnShellInitialized;
        _hookManager.OnWindowHookToggled += HookManager_OnWindowHookToggled;
        _hookManager.OnReuseTabsToggled += HookManager_OnReuseTabsToggled;

        // Populate submenus for keyboard & mouse profiles
        UpdateMenuItems(autoCheckParent: false);
    }

    private void InitializeCommands()
    {
        ProfileItemCommand = new RelayCommand(OnProfileItemClick, s => s != null && ((MenuItem)s).IsEnabled);

        KeyboardHookMenu.CommandParameter = KeyboardHookMenu;
        KeyboardHookMenu.Command = new RelayCommand(_ => ToggleKeyboardHookMenu(), s => s != null && ((MenuItem)s).HasItems);

        MouseHookMenu.CommandParameter = MouseHookMenu;
        MouseHookMenu.Command = new RelayCommand(_ => ToggleMouseHookMenu(), s => s != null && ((MenuItem)s).HasItems);

        WindowHook.Command = new RelayCommand(_ => ToggleWindowHook());
        ReuseTabs.Command = new RelayCommand(_ => ToggleReuseTabs());
        AddToStartup.Command = new RelayCommand(_ => ToggleStartup());
        OpenSettings.Command = new RelayCommand(_ => _showWindowAction());
        CheckForUpdates.Command = new RelayCommand(_ => UpdateManager.CheckForUpdates());
        ExitApplication.Command = new RelayCommand(_ => Application.Current.Shutdown());
    }

    private void HookManager_OnShellInitialized()
    {
        // Workaround for hardcodet/wpf-notifyicon bug: repeatedly hide icon when explorer.exe restarts
        if (TrayIcon.Visibility == Visibility.Visible) return;
        Helper.DoUntilTimeEnd(HideTrayIcon, 7000, 1000);
        return;
        
        void HideTrayIcon()
        {
            TrayIcon.Dispatcher.BeginInvoke(() =>
            {
                if (TrayIcon.Visibility == Visibility.Visible) return;
                TrayIcon.Visibility = Visibility.Hidden;
                TrayIcon.Visibility = Visibility.Collapsed;
            });
        }
    }
    
    private void HookManager_OnWindowHookToggled()
    {
        if (WindowHook.IsChecked)
        {
            // Store the current ReuseTabs state before toggling WindowHook
            _savedReuseTabsState = SettingsManager.ReuseTabs;

            WindowHook.IsChecked = false;
            WindowHook.Command.Execute(WindowHook.CommandParameter);
            return;
        }
        
        if (_savedReuseTabsState)
        {
            // It will activate window hook too as well.
            ReuseTabs.IsChecked = true;
            ReuseTabs.Command.Execute(ReuseTabs.CommandParameter);
        }
        else
        {
            WindowHook.IsChecked = true;
            WindowHook.Command.Execute(WindowHook.CommandParameter);
        }
    }

    private void HookManager_OnReuseTabsToggled()
    {
        ReuseTabs.IsChecked = !ReuseTabs.IsChecked;
        ReuseTabs.Command.Execute(ReuseTabs.CommandParameter);
    }

    public void UpdateMenuItems(bool autoCheckParent = true)
    {
        PopulateHookProfiles(KeyboardHookMenu, _profileManager.GetKeyboardProfiles(), autoCheckParent);
        PopulateHookProfiles(MouseHookMenu, _profileManager.GetMouseProfiles(), autoCheckParent);
    }

    public void SetTrayIconVisibility(bool visible) => TrayIcon.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    private void OnNotifyIconDoubleClick(object sender, RoutedEventArgs _) => _showWindowAction();

    private void ToggleKeyboardHookMenu()
    {
        KeyboardHookMenu.IsChecked = !KeyboardHookMenu.IsChecked;

        ToggleHookState(KeyboardHookMenu,
            v => SettingsManager.IsKeyboardHookActive = v,
            _hookManager.StartKeyboardHook,
            _hookManager.StopKeyboardHook
        );
    }

    private void ToggleMouseHookMenu()
    {
        MouseHookMenu.IsChecked = !MouseHookMenu.IsChecked;

        ToggleHookState(MouseHookMenu,
            v => SettingsManager.IsMouseHookActive = v,
            _hookManager.StartMouseHook,
            _hookManager.StopMouseHook
        );
    }

    private void ToggleWindowHook()
    {
        ToggleHookState(WindowHook,
            v => SettingsManager.IsWindowHookActive = v,
            _hookManager.StartWindowHook,
            _hookManager.StopWindowHook
        );

        if (!WindowHook.IsChecked && ReuseTabs.IsChecked)
        {
            ReuseTabs.IsChecked = false;
            ReuseTabs.Command.Execute(ReuseTabs.CommandParameter);
        }
    }

    private static void ToggleHookState(MenuItem parent, Action<bool> setSetting, Action startHook, Action stopHook)
    {
        setSetting(parent.IsChecked);
        (parent.IsChecked ? startHook : stopHook)();

        if (parent.Name?.EndsWith("Menu") != true) return;

        // Enable/disable dropdown menu based on current state
        foreach (MenuItem dropDownItem in parent.Items)
            dropDownItem.IsEnabled = parent.IsChecked;

        // If hook is enabled and no items are checked, tick the first one
        if (parent.IsChecked && !parent.Items.Cast<MenuItem>().Any(i => i.IsChecked))
        {
            var first = (MenuItem)parent.Items[0]!;
            first.IsChecked = !first.IsChecked;
            first.Command.Execute(first.CommandParameter);
        }

        // Force menu to close and reopen to refresh the UI state
        var isOpen = parent.IsSubmenuOpen;
        parent.IsSubmenuOpen = false;
        Application.Current.Dispatcher.InvokeAsync(() => { parent.IsSubmenuOpen = isOpen; },
            System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }

    private void ToggleReuseTabs()
    {
        SettingsManager.ReuseTabs = ReuseTabs.IsChecked;
        _hookManager.SetReuseTabs(ReuseTabs.IsChecked);

        if (ReuseTabs.IsChecked && !WindowHook.IsChecked)
        {
            WindowHook.IsChecked = true;
            WindowHook.Command.Execute(WindowHook.CommandParameter);
        }
    }

    private void ToggleStartup()
    {
        RegistryManager.ToggleStartup();
        AddToStartup.IsChecked = RegistryManager.IsStartupEnabled;
    }

    private void PopulateHookProfiles(MenuItem parent, IEnumerable<HotKeyProfile> profiles, bool autoCheckParent = true)
    {
        parent.Items.Clear();

        foreach (var profile in profiles)
        {
            var profileItem = new MenuItem
            {
                Header = profile.Name,
                StaysOpenOnClick = true,
                IsCheckable = true,
                IsChecked = profile.IsEnabled,
                IsEnabled = parent.IsChecked,
                Command = ProfileItemCommand,
                Tag = profile
            };

            profileItem.CommandParameter = profileItem;
            parent.Items.Add(profileItem);
        }

        var anyChecked = parent.Items.Cast<MenuItem>().Any(item => item.IsChecked);

        // No subitems are checked, uncheck the parent.
        // At least one subitem is checked, check the parent (if autoCheckParent)
        var desiredParentChecked = anyChecked && (parent.IsChecked || autoCheckParent);
        if (desiredParentChecked != parent.IsChecked)
            parent.Command.Execute(parent.CommandParameter);
    }

    private void OnProfileItemClick(object? sender)
    {
        if (sender is not MenuItem { Tag: HotKeyProfile profile } item) return;

        _profileManager.SetProfileEnabledFromTray(profile, item.IsChecked);

        if (item.Parent is not MenuItem { IsChecked: true } parent) return;

        var anyChecked = parent.Items.OfType<MenuItem>().Any(m => m.IsChecked);
        if (anyChecked) return;

        // No subitems are checked, uncheck the parent
        parent.Command.Execute(parent.CommandParameter);
    }

    public void Dispose()
    {
        TrayIcon.Dispose();
        GC.SuppressFinalize(this);
    }
}