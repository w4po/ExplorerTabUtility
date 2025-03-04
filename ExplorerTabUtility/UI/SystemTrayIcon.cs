using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.UI;

public class SystemTrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ProfileManager _profileManager;
    private readonly HookManager _hookManager;
    private readonly Action _showFormAction;

    public SystemTrayIcon(ProfileManager profileManager, HookManager hookManager, Action showFormAction)
    {
        _profileManager = profileManager;
        _hookManager = hookManager;
        _showFormAction = showFormAction;

        _notifyIcon = new NotifyIcon
        {
            Icon = Helper.GetIcon(),
            Text = Constants.NotifyIconText,
            ContextMenuStrip = CreateContextMenuStrip(firstRun: true),
            Visible = true
        };
        _notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;

        // Subscribe to profile changes to update menu
        _profileManager.ProfilesChanged += UpdateMenuItems;

        _hookManager.OnWindowHookToggled += OnWindowHookToggled;
        _hookManager.OnReuseTabsToggled += OnReuseTabsToggled;
    }

    public void UpdateMenuItems() => _notifyIcon.ContextMenuStrip = CreateContextMenuStrip();
    public void SetTrayIconVisibility(bool visible) => _notifyIcon.Visible = visible;
    private void OnNotifyIconDoubleClick(object? _, MouseEventArgs e) => _showFormAction();
    private void OnWindowHookToggled() => GetMenuItemByKey("WindowHook")!.PerformClick();
    private void OnReuseTabsToggled() => GetMenuItemByKey("ReuseTabs")!.PerformClick();

    private ContextMenuStrip CreateContextMenuStrip(bool firstRun = false)
    {
        var menuStrip = new ContextMenuStrip();
        menuStrip.Items.AddRange(
        [
            CreateHookMenuItem("Keyboard Hook", SettingsManager.IsKeyboardHookActive, ToggleKeyboardHook, "KeyboardHookMenu", firstRun),
            CreateHookMenuItem("Mouse Hook", SettingsManager.IsMouseHookActive, ToggleMouseHook, "MouseHookMenu", firstRun),
            CreateHookMenuItem("Window Hook", SettingsManager.IsWindowHookActive, ToggleWindowHook, "WindowHook"),
            CreateHookMenuItem("Reuse Tabs", SettingsManager.ReuseTabs, ToggleReuseTabs, "ReuseTabs"),
            new ToolStripSeparator(),
            CreateMenuItem("Add to startup", RegistryManager.IsInStartup(), static (_, _) => RegistryManager.ToggleStartup()),
            CreateMenuItem("Check for updates", false, (_, _) => UpdateManager.CheckForUpdates(), checkOnClick: false),
            CreateMenuItem("Settings", false, (_, _) => _showFormAction(), checkOnClick: false),
            new ToolStripSeparator(),
            CreateMenuItem("Exit", false, static (_, _) => System.Windows.Application.Current.Shutdown())
        ]);
        return menuStrip;
    }

    private ToolStripMenuItem CreateHookMenuItem(string text, bool isActive, EventHandler handler, string key, bool firstRun = false)
    {
        var menuItem = CreateMenuItem(text, isActive, handler, key);
        if (!key.EndsWith("Menu")) return menuItem;

        var profiles = key.StartsWith("Keyboard")
            ? _profileManager.GetKeyboardProfiles()
            : _profileManager.GetMouseProfiles();

        AddProfilesToMenuItem(menuItem, profiles, firstRun);

        // Enable/disable dropdown menu based on current state
        foreach (ToolStripMenuItem dropDownItem in menuItem.DropDownItems)
            dropDownItem.Enabled = menuItem.Checked;

        return menuItem;
    }

    private void AddProfilesToMenuItem(ToolStripMenuItem parent, IEnumerable<HotKeyProfile> profiles, bool firstRun = false)
    {
        foreach (var profile in profiles)
        {
            var profileItem = new ToolStripMenuItem(profile.Name)
            {
                Checked = profile.IsEnabled,
                CheckOnClick = true,
                Tag = profile
            };
            profileItem.Click += OnProfileItemClick;
            parent.DropDownItems.Add(profileItem);
        }

        var hasEnabledProfiles = parent.DropDownItems.Cast<ToolStripMenuItem>().Any(item => item.Checked);
        if (!hasEnabledProfiles && parent.Checked)
            parent.PerformClick();
        else if (hasEnabledProfiles && !parent.Checked && !firstRun)
            parent.PerformClick();
    }

    private void OnProfileItemClick(object? s, EventArgs _)
    {
        var item = (ToolStripMenuItem)s!;
        var profile = (HotKeyProfile)item.Tag!;
        _profileManager.SetProfileEnabledFromTray(profile, item.Checked);

        if (item.OwnerItem is not ToolStripMenuItem parent) return;

        // Stop the hook if this was the last item checked
        var hasEnabledProfiles = parent.DropDownItems.Cast<ToolStripMenuItem>().Any(i => i.Checked);
        if (parent.Checked != hasEnabledProfiles)
            parent.PerformClick();
    }

    private void ToggleHookState(object? sender, Action<bool> setSetting, Action startHook, Action stopHook)
    {
        if (sender is not ToolStripMenuItem item) return;

        setSetting(item.Checked);
        (item.Checked ? startHook : stopHook)();

        if (item.Name?.EndsWith("Menu") != true) return;

        if (item is { Checked: true, DropDownItems.Count: 0 })
        {
            item.PerformClick();
            return;
        }

        // Enable/disable dropdown menu based on current state
        foreach (ToolStripMenuItem dropDownItem in item.DropDownItems)
            dropDownItem.Enabled = item.Checked;

        // If hook is enabled and no items are checked, tick the first one
        if (item.Checked && !item.DropDownItems.Cast<ToolStripMenuItem>().Any(i => i.Checked))
            item.DropDownItems[0].PerformClick();
    }

    private void ToggleKeyboardHook(object? sender, EventArgs e) =>
        ToggleHookState(sender,
            v => SettingsManager.IsKeyboardHookActive = v,
            _hookManager.StartKeyboardHook,
            _hookManager.StopKeyboardHook
        );

    private void ToggleMouseHook(object? sender, EventArgs e) =>
        ToggleHookState(sender,
            v => SettingsManager.IsMouseHookActive = v,
            _hookManager.StartMouseHook,
            _hookManager.StopMouseHook
        );

    private void ToggleWindowHook(object? sender, EventArgs e) =>
        ToggleHookState(sender,
            v => SettingsManager.IsWindowHookActive = v,
            _hookManager.StartWindowHook,
            _hookManager.StopWindowHook
        );

    private void ToggleReuseTabs(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        SettingsManager.ReuseTabs = item.Checked;
        _hookManager.SetReuseTabs(item.Checked);
    }

    private static ToolStripMenuItem CreateMenuItem(string text, bool isChecked, EventHandler onClick, string? key = null, bool checkOnClick = true)
    {
        var item = new ToolStripMenuItem
        {
            Text = text,
            Checked = isChecked,
            CheckOnClick = checkOnClick,
            Name = key
        };
        item.Click += onClick;
        return item;
    }

    private ToolStripMenuItem? GetMenuItemByKey(string key)
    {
        var item = _notifyIcon.ContextMenuStrip!.Items.Find(key, false).FirstOrDefault();
        return item as ToolStripMenuItem;
    }

    public void Dispose()
    {
        _notifyIcon.Dispose();
        GC.SuppressFinalize(this);
    }
}