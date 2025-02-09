using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Forms;

public partial class MainForm : MaterialForm
{
    private readonly HookManager _hookManager;
    private readonly ProfileManager _profileManager;
    private readonly SystemTrayIcon _notifyIconManager;

    public MainForm()
    {
        InitializeComponent();
        SetupMaterialSkin();

        _profileManager = new ProfileManager(flpProfiles);
        _hookManager = new HookManager(_profileManager);
        _notifyIconManager = new SystemTrayIcon(_profileManager, _hookManager, ShowForm);

        SetupEventHandlers();
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

    private void SetupEventHandlers()
    {
        Application.ApplicationExit += OnApplicationExit;
        _hookManager.OnVisibilityToggled += ToggleFormVisibility;
    }

    private void StartHooks()
    {
        if (SettingsManager.IsWindowHookActive) _hookManager.StartWindowHook();
        if (SettingsManager.IsMouseHookActive) _hookManager.StartMouseHook();
        if (SettingsManager.IsKeyboardHookActive) _hookManager.StartKeyboardHook();
        _hookManager.SetReuseTabs(SettingsManager.ReuseTabs);
    }

    private void BtnNewProfile_Click(object _, EventArgs __) => _profileManager.AddProfile();

    private void BtnImport_Click(object _, EventArgs __)
    {
        using var ofd = new OpenFileDialog();
        ofd.FileName = Constants.HotKeyProfilesFileName;
        ofd.Filter = Constants.JsonFileFilter;

        if (ofd.ShowDialog() != DialogResult.OK) return;
        var jsonString = System.IO.File.ReadAllText(ofd.FileName);
        _profileManager.ImportProfiles(jsonString);
        _notifyIconManager.UpdateMenuItems();
    }

    private void BtnExport_Click(object _, EventArgs __)
    {
        using var sfd = new SaveFileDialog();
        sfd.FileName = Constants.HotKeyProfilesFileName;
        sfd.Filter = Constants.JsonFileFilter;

        if (sfd.ShowDialog() != DialogResult.OK) return;
        using var openFile = sfd.OpenFile();
        var jsonString = _profileManager.ExportProfiles();
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        openFile.Write(bytes, 0, bytes.Length);
    }

    private void BtnSave_Click(object _, EventArgs __)
    {
        _profileManager.SaveProfiles();
        _notifyIconManager.UpdateMenuItems();
    }

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
        _notifyIconManager.Dispose();
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
            {
                _profileManager.SaveProfiles();
                _notifyIconManager.UpdateMenuItems();
            }
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
            {
                _profileManager.SaveProfiles();
                _notifyIconManager.UpdateMenuItems();
            }
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