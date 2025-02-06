using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Collections.Generic;
using MaterialSkin;
using MaterialSkin.Controls;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Managers;

namespace ExplorerTabUtility.Forms;

public partial class MainForm : MaterialForm
{
    private readonly HookManager _hookManager;
    private readonly List<HotKeyProfile> _hotKeyProfiles = [];
    private NotifyIcon _notifyIcon = null!;

    public MainForm()
    {
        Application.ApplicationExit += OnApplicationExit;

        _hookManager = new HookManager(_hotKeyProfiles);
        _hookManager.OnVisibilityToggled += ToggleFormVisibility;
        _hookManager.OnWindowHookToggled += () => Invoke(() => GetMenuItemByKey("WindowHook")!.PerformClick());
        _hookManager.OnReuseTabsToggled += () => Invoke(() => GetMenuItemByKey("ReuseTabs")!.PerformClick());

        InitializeComponent();
        SetupMaterialSkin();
        InitializeNotifyIcon();
        StartHooks();
    }

    private void SetupMaterialSkin()
    {
        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.EnforceBackcolorOnAllComponents = false;
        materialSkinManager.AddFormToManage(this);
        materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
        materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.LightBlue600, Accent.LightBlue200, TextShade.WHITE);
    }
    private void InitializeNotifyIcon()
    {
        var importedList = DeserializeHotKeyProfiles(SettingsManager.HotKeyProfiles);
        if (importedList != null) AddProfiles(importedList);

        _notifyIcon = new NotifyIcon
        {
            Icon = Helper.GetIcon(),
            Text = Constants.NotifyIconText,

            ContextMenuStrip = CreateContextMenuStrip(),
            Visible = true
        };

        // Show the form when the user double-clicks on the notify icon.
        _notifyIcon.MouseDoubleClick += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            ShowForm();
        };
    }
    private void StartHooks()
    {
        if (SettingsManager.IsWindowHookActive) _hookManager.StartWindowHook();
        if (SettingsManager.IsMouseHookActive) _hookManager.StartMouseHook();
        if (SettingsManager.IsKeyboardHookActive) _hookManager.StartKeyboardHook();

        _hookManager.SetReuseTabs(SettingsManager.ReuseTabs);
    }

    private ContextMenuStrip CreateContextMenuStrip()
    {
        var strip = new ContextMenuStrip();

        // KeyboardHook Menu
        strip.Items.Add(CreateKeyboardHookMenuItem());

        // MouseHook Menu
        strip.Items.Add(CreateMouseHookMenuItem());

        // WindowHook
        strip.Items.Add(CreateMenuItem("Window Hook", SettingsManager.IsWindowHookActive, ToggleWindowHook, "WindowHook"));

        // WindowHook
        strip.Items.Add(CreateMenuItem("ReuseTabs", SettingsManager.ReuseTabs, ToggleReuseTabs, "ReuseTabs"));

        // Separator
        strip.Items.Add(new ToolStripSeparator());

        // Startup
        strip.Items.Add(CreateMenuItem("Add to startup", RegistryManager.IsInStartup(), static (_, _) => RegistryManager.ToggleStartup()));

        // Settings
        strip.Items.Add(CreateMenuItem("Settings", false, (_, _) => ShowForm(), checkOnClick: false));

        // Separator
        strip.Items.Add(new ToolStripSeparator());

        // Exit
        strip.Items.Add(CreateMenuItem("Exit", false, static (_, _) => Application.Exit()));

        return strip;
    }

    private ToolStripItem? GetMenuItemByKey(string key)
    {
        return _notifyIcon.ContextMenuStrip!.Items[key];
    }
    private ToolStripMenuItem CreateKeyboardHookMenuItem()
    {
        var menuItem = CreateMenuItem("Keyboard Hook", SettingsManager.IsKeyboardHookActive, ToggleKeyboardHook, "KeyboardHookMenu");

        AddKeyboardProfilesToMenuItem(menuItem);
        return menuItem;
    }
    private ToolStripMenuItem CreateMouseHookMenuItem()
    {
        var menuItem = CreateMenuItem("Mouse Hook", SettingsManager.IsMouseHookActive, ToggleMouseHook, "MouseHookMenu");

        AddMouseProfilesToMenuItem(menuItem);
        return menuItem;
    }
    private void UpdateKeyboardHookMenu()
    {
        var menuItem = GetMenuItemByKey("KeyboardHookMenu") as ToolStripMenuItem;

        menuItem!.DropDownItems.Clear();
        AddKeyboardProfilesToMenuItem(menuItem);
    }
    private void UpdateMouseHookMenu()
    {
        var menuItem = GetMenuItemByKey("MouseHookMenu") as ToolStripMenuItem;

        menuItem!.DropDownItems.Clear();
        AddMouseProfilesToMenuItem(menuItem);
    }
    private void AddKeyboardProfilesToMenuItem(ToolStripMenuItem menuItem)
    {
        foreach (var profile in _hotKeyProfiles.Where(p => !p.IsMouse))
        {
            var profileMenuItem = CreateMenuItem(profile.Name ?? string.Empty, profile.IsEnabled, eventHandler: KeyboardHookProfileItemClick);
            profileMenuItem.Tag = profile;
            menuItem.DropDownItems.Add(profileMenuItem);
        }

        // if the hook is active and there not a single profile enabled, deactivate the hook
        if (SettingsManager.IsKeyboardHookActive && !_hotKeyProfiles.Any(p => p.IsEnabled && !p.IsMouse))
            menuItem.PerformClick();
    }
    private void AddMouseProfilesToMenuItem(ToolStripMenuItem menuItem)
    {
        foreach (var profile in _hotKeyProfiles.Where(p => p.IsMouse))
        {
            var profileMenuItem = CreateMenuItem(profile.Name ?? string.Empty, profile.IsEnabled, eventHandler: MouseHookProfileItemClick);
            profileMenuItem.Tag = profile;
            menuItem.DropDownItems.Add(profileMenuItem);
        }

        // if the hook is active and there not a single profile enabled, deactivate the hook
        if (SettingsManager.IsMouseHookActive && !_hotKeyProfiles.Any(p => p.IsEnabled && p.IsMouse))
            menuItem.PerformClick();
    }
    private static ToolStripMenuItem CreateMenuItem(string text, bool isChecked = false, EventHandler? eventHandler = null,
        string? name = null, bool checkOnClick = true, params ToolStripItem[] dropDownItems)
    {
        var item = new ToolStripMenuItem
        {
            Text = text,
            Checked = isChecked,
            CheckOnClick = checkOnClick
        };

        if (name != null)
            item.Name = name;

        if (eventHandler != null)
            item.Click += eventHandler;

        if (dropDownItems.Length > 0)
            item.DropDownItems.AddRange(dropDownItems);

        return item;
    }
    
    private void UpdateMenuAndSaveProfiles()
    {
        // Remove profiles that don't have any hotkeys.
        _hotKeyProfiles.FindAll(p => p.HotKeys == null || p.HotKeys.Length == 0).ForEach(RemoveProfile);

        UpdateKeyboardHookMenu();
        UpdateMouseHookMenu();

        SettingsManager.HotKeyProfiles = JsonSerializer.Serialize(_hotKeyProfiles);
    }
    private void AddProfiles(List<HotKeyProfile> profiles, bool clear = false)
    {
        flpProfiles.SuspendLayout();
        flpProfiles.SuspendDrawing();

        if (clear) ClearProfiles();

        profiles.ForEach(AddProfile);

        flpProfiles.ResumeDrawing();
        flpProfiles.ResumeLayout();
    }
    private void AddProfile(HotKeyProfile? profile = null)
    {
        _hotKeyProfiles.Add(profile ??= new HotKeyProfile());

        // We have to stop the main keyboard/mouse hook when we edit a profile key bindings (ControlStartedKeybindingHook, ControlStoppedKeybindingHook)
        flpProfiles.Controls.Add(new HotKeyProfileControl(profile, RemoveProfile, ControlStartedKeybindingHook, ControlStoppedKeybindingHook));
    }
    private void ClearProfiles()
    {
        _hotKeyProfiles.Clear();
        flpProfiles.Controls.Clear();
    }
    private void RemoveProfile(HotKeyProfile profile)
    {
        _hotKeyProfiles.Remove(profile);

        var control = FindControlByProfile(profile);
        if (control != null)
            flpProfiles.Controls.Remove(control);
    }
    private HotKeyProfileControl? FindControlByProfile(HotKeyProfile profile)
    {
        return flpProfiles.Controls
            .OfType<HotKeyProfileControl>()
            .FirstOrDefault(c => c.Tag?.Equals(profile) == true);
    }
    private static List<HotKeyProfile>? DeserializeHotKeyProfiles(string jsonString)
    {
        try
        {
            return JsonSerializer.Deserialize<List<HotKeyProfile>>(jsonString);
        }
        catch
        {
            return null;
        }
    }

    private void ControlStartedKeybindingHook()
    {
        if (SettingsManager.IsKeyboardHookActive)
            _hookManager.StopKeyboardHook();

        if (SettingsManager.IsMouseHookActive)
            _hookManager.StopMouseHook();
    }
    private void ControlStoppedKeybindingHook()
    {
        if (SettingsManager.IsKeyboardHookActive)
            _hookManager.StartKeyboardHook();

        if (SettingsManager.IsMouseHookActive)
            _hookManager.StartMouseHook();
    }
    private void KeyboardHookProfileItemClick(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item || item.OwnerItem is not ToolStripMenuItem parent) return;

        // Toggle the HotKeyProfileControl's Enabled state.
        if (item.Tag is HotKeyProfile profile)
        {
            var control = FindControlByProfile(profile);
            if (control != null) control.IsEnabled = item.Checked;
        }

        // Uncheck parent if all sub items are unchecked.
        parent.Checked = parent.DropDownItems.OfType<ToolStripMenuItem>().Any(c => c.Checked);
        if (!parent.Checked)
            ToggleKeyboardHook(parent, EventArgs.Empty);
    }
    private void MouseHookProfileItemClick(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item || item.OwnerItem is not ToolStripMenuItem parent) return;

        // Toggle the HotKeyProfileControl's Enabled state.
        if (item.Tag is HotKeyProfile profile)
        {
            var control = FindControlByProfile(profile);
            if (control != null) control.IsEnabled = item.Checked;
        }

        // Uncheck parent if all sub items are unchecked.
        parent.Checked = parent.DropDownItems.OfType<ToolStripMenuItem>().Any(c => c.Checked);
        if (!parent.Checked)
            ToggleMouseHook(parent, EventArgs.Empty);
    }
    private void ToggleKeyboardHook(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        var keyboardOnly = _hotKeyProfiles.Where(p => !p.IsMouse).ToList();
        if (item.Checked && keyboardOnly.Count == 0)
        {
            item.Checked = false;
            _hookManager.StopKeyboardHook();
            return;
        }

        SettingsManager.IsKeyboardHookActive = item.Checked;

        foreach (ToolStripItem subItem in item.DropDownItems)
            subItem.Enabled = item.Checked;

        if (item.Checked)
        {
            // If all sub items are not checked, click the first item.
            if (keyboardOnly.TrueForAll(h => !h.IsEnabled))
                item.DropDownItems[0].PerformClick();

            _hookManager.StartKeyboardHook();
        }
        else
            _hookManager.StopKeyboardHook();
    }
    private void ToggleMouseHook(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        var mouseOnly = _hotKeyProfiles.Where(p => p.IsMouse).ToList();
        if (item.Checked && mouseOnly.Count == 0)
        {
            item.Checked = false;
            _hookManager.StopMouseHook();
            return;
        }

        SettingsManager.IsMouseHookActive = item.Checked;

        foreach (ToolStripItem subItem in item.DropDownItems)
            subItem.Enabled = item.Checked;

        if (item.Checked)
        {
            // If all sub items are not checked, click the first item.
            if (mouseOnly.TrueForAll(h => !h.IsEnabled))
                item.DropDownItems[0].PerformClick();

            _hookManager.StartMouseHook();
        }
        else
            _hookManager.StopMouseHook();
    }
    private void ToggleWindowHook(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        SettingsManager.IsWindowHookActive = item.Checked;

        if (item.Checked)
            _hookManager.StartWindowHook();
        else
            _hookManager.StopWindowHook();
    }
    private void ToggleReuseTabs(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        SettingsManager.ReuseTabs = item.Checked;
        _hookManager.SetReuseTabs(item.Checked);
    }
    private void BtnNewProfile_Click(object _, EventArgs __) => AddProfile();
    private void BtnImport_Click(object _, EventArgs __)
    {
        using var ofd = new OpenFileDialog();
        ofd.FileName = Constants.HotKeyProfilesFileName;
        ofd.Filter = Constants.JsonFileFilter;
        if (ofd.ShowDialog() != DialogResult.OK) return;

        var jsonString = System.IO.File.ReadAllText(ofd.FileName);
        var importedList = DeserializeHotKeyProfiles(jsonString);
        if (importedList == null) return;

        AddProfiles(importedList, true);
    }
    private void BtnExport_Click(object _, EventArgs __)
    {
        using var sfd = new SaveFileDialog();
        sfd.FileName = Constants.HotKeyProfilesFileName;
        sfd.Filter = Constants.JsonFileFilter;
        if (sfd.ShowDialog() != DialogResult.OK) return;

        using var openFile = sfd.OpenFile();
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_hotKeyProfiles));
        openFile.Write(bytes, 0, bytes.Length);
    }
    private void BtnSave_Click(object _, EventArgs __) => UpdateMenuAndSaveProfiles();
    private void CbSaveProfilesOnExit_CheckedChanged(object _, EventArgs __)
    {
        SettingsManager.SaveProfilesOnExit = cbSaveProfilesOnExit.Checked;
    }
    private void FlpProfiles_Resize(object _, EventArgs __)
    {
        foreach (Control c in flpProfiles.Controls)
            c.Width = flpProfiles.Width - 25;
    }
    private void MainForm_Deactivate(object _, EventArgs __) => flpProfiles.Focus();
    private void OnApplicationExit(object? _, EventArgs __)
    {
        _notifyIcon.Visible = false;
        _hookManager.Dispose();
    }

    private void ShowForm()
    {
        WinApi.ShowWindow(Handle, WinApi.SW_SHOWNOACTIVATE);
        WinApi.SetForegroundWindow(Handle);
    }
    private void ToggleFormVisibility()
    {
        Invoke(() =>
        {
            if (Visible)
                Hide();
            else
                ShowForm();
        });
    }
    protected override void OnResize(EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            
            if (cbSaveProfilesOnExit.Checked)
                UpdateMenuAndSaveProfiles();
        }

        base.OnResize(e);
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            
            if (cbSaveProfilesOnExit.Checked)
                UpdateMenuAndSaveProfiles();
        }
        base.OnFormClosing(e);
    }
    protected override void SetVisibleCore(bool value)
    {
        if (!IsHandleCreated)
        {
            value = false;
            CreateHandle();
        }
        base.SetVisibleCore(value);
    }
}