﻿using System;
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
    private bool _allowVisible;
    private NotifyIcon _notifyIcon = null!;

    public MainForm()
    {
        Application.ApplicationExit += OnApplicationExit;

        _hookManager = new HookManager(_hotKeyProfiles);

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
        if (SettingsManager.IsKeyboardHookActive) _hookManager.StartKeyboardHook();
    }

    private ContextMenuStrip CreateContextMenuStrip()
    {
        var strip = new ContextMenuStrip();

        // KeyboardHook Menu
        strip.Items.Add(CreateKeyboardHookMenuItem());

        // WindowHook
        strip.Items.Add(CreateMenuItem("Window Hook", SettingsManager.IsWindowHookActive, ToggleWindowHook));

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
    private ToolStripMenuItem CreateKeyboardHookMenuItem()
    {
        var menuItem = CreateMenuItem("Keyboard Hook", SettingsManager.IsKeyboardHookActive, ToggleKeyboardHook, "KeyboardHookMenu");

        AddProfilesToMenuItem(menuItem);
        return menuItem;
    }
    private void UpdateKeyboardHookMenu()
    {
        var menuItem = (ToolStripMenuItem?)_notifyIcon.ContextMenuStrip?.Items["KeyboardHookMenu"];
        if (menuItem == null) return;

        menuItem.DropDownItems.Clear();
        AddProfilesToMenuItem(menuItem);
    }
    private void AddProfilesToMenuItem(ToolStripMenuItem menuItem)
    {
        foreach (var profile in _hotKeyProfiles)
        {
            var profileMenuItem = CreateMenuItem(profile.Name ?? string.Empty, profile.IsEnabled, eventHandler: KeyboardHookProfileItemClick);
            profileMenuItem.Tag = profile;
            menuItem.DropDownItems.Add(profileMenuItem);
        }

        // if the hook is active and there not a single profile enabled, deactivate the hook
        if (SettingsManager.IsKeyboardHookActive && !_hotKeyProfiles.Any(p => p.IsEnabled))
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

        // We have to stop the main keyboard hook when we edit a profile key bindings (ControlStartedKeyboardHook, ControlStoppedKeyboardHook)
        flpProfiles.Controls.Add(new HotKeyProfileControl(profile, RemoveProfile, ControlStartedKeyboardHook, ControlStoppedKeyboardHook));
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

    private void ControlStartedKeyboardHook()
    {
        if (!SettingsManager.IsKeyboardHookActive) return;
        _hookManager.StopKeyboardHook();
    }
    private void ControlStoppedKeyboardHook()
    {
        if (!SettingsManager.IsKeyboardHookActive) return;
        _hookManager.StartKeyboardHook();
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
    private void ToggleKeyboardHook(object? sender, EventArgs _)
    {
        if (sender is not ToolStripMenuItem item) return;

        if (item.Checked && _hotKeyProfiles.Count == 0)
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
            if (_hotKeyProfiles.TrueForAll(h => !h.IsEnabled))
                item.DropDownItems[0].PerformClick();

            _hookManager.StartKeyboardHook();
        }
        else
            _hookManager.StopKeyboardHook();
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
    private void CbSaveProfilesOnExit_CheckedChanged(object _, EventArgs __) => SettingsManager.SaveProfilesOnExit = cbSaveProfilesOnExit.Checked;
    private void FlpProfiles_Resize(object _, EventArgs __)
    {
        foreach (Control c in flpProfiles.Controls)
            c.Width = flpProfiles.Width - 20;
    }
    private void MainForm_Resize(object _, EventArgs __)
    {
        if (WindowState == FormWindowState.Normal)
        {
            _allowVisible = true;
        }
        else if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
            _allowVisible = false;
            Hide();

            if (!cbSaveProfilesOnExit.Checked) return;

            // Save
            UpdateMenuAndSaveProfiles();
        }
    }
    private void MainForm_FormClosing(object _, FormClosingEventArgs e)
    {
        if (e.CloseReason != CloseReason.UserClosing) return;

        e.Cancel = true;
        _allowVisible = false;

        // Hide the form instead of closing it when the close button is clicked
        Hide();

        if (!cbSaveProfilesOnExit.Checked) return;

        // Save
        UpdateMenuAndSaveProfiles();
    }
    private void MainForm_Deactivate(object _, EventArgs __) => flpProfiles.Focus();
    private void OnApplicationExit(object? _, EventArgs __)
    {
        _notifyIcon.Visible = false;
        _hookManager.Dispose();
    }

    private void ShowForm()
    {
        _allowVisible = true;
        WindowState = FormWindowState.Normal;
        Show();
    }
    protected override void SetVisibleCore(bool value)
    {
        if (!_allowVisible)
        {
            value = false;
            if (!IsHandleCreated) CreateHandle();
        }
        base.SetVisibleCore(value);
    }
}