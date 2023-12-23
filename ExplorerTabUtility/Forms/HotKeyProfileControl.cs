using System;
using System.Linq;
using System.Windows.Forms;
using ExplorerTabUtility.Models;
using H.Hooks;

namespace ExplorerTabUtility.Forms;

public partial class HotKeyProfileControl : UserControl
{
    // Fields
    private bool _isCollapsed;
    private readonly int _collapsedHeight;
    private readonly HotKeyProfile _profile;
    private readonly Action<HotKeyProfile>? _removeAction;
    private readonly Action? _keyboardHookStarted;
    private readonly Action? _keyboardHookStopped;
    private LowLevelKeyboardHook? _lowLevelKeyboardHook;
    // Constants
    private const int ExpandedHeight = 76;
    // Properties
    public bool IsEnabled
    {
        get => cbEnabled.Checked;
        set
        {
            if (cbEnabled.Checked == value) return;
            cbEnabled.Checked = value;
        }
    }
    public bool IsCollapsed
    {
        get => _isCollapsed;
        set
        {
            if (_isCollapsed == value) return;
            _isCollapsed = value;
            ToggleCollapse();
        }
    }

    // Constructor
    public HotKeyProfileControl(HotKeyProfile profile, Action<HotKeyProfile>? removeAction = default, Action? keyboardHookStarted = default, Action? keyboardHookStopped = default)
    {
        InitializeComponent();
        Tag = profile;
        _profile = profile;
        _collapsedHeight = Height;
        _removeAction = removeAction;
        _keyboardHookStarted = keyboardHookStarted;
        _keyboardHookStopped = keyboardHookStopped;
        InitializeControls();
    }
    // Initialize controls with hot key profile data
    private void InitializeControls()
    {
        cbEnabled.Checked = _profile.IsEnabled;
        txtName.Text = _profile.Name ?? string.Empty;

        if (_profile.HotKeys != null)
            txtHotKeys.Text = string.Join(" + ", _profile.HotKeys.Select(k => k.ToFixedString()));

        SetComboBoxDataSourceQuietly(cbScope, Enum.GetValues(typeof(HotkeyScope)), CbScope_SelectedIndexChanged);
        SetComboBoxDataSourceQuietly(cbAction, Enum.GetValues(typeof(HotKeyAction)), CbAction_SelectedIndexChanged);
        cbScope.SelectedItem = _profile.Scope;
        cbAction.SelectedItem = _profile.Action;
        txtPath.Text = _profile.Path ?? string.Empty;
        sDelay.Value = _profile.Delay;
        cbHandled.Checked = _profile.IsHandled;
    }

    // Event handlers
    private void CbEnabled_CheckedChanged(object? _, EventArgs __) => UpdateControlsEnabledState();
    private void TxtName_TextChanged(object? _, EventArgs __) => _profile.Name = txtName.Text;
    private void CbScope_SelectedIndexChanged(object? _, EventArgs __) => _profile.Scope = (HotkeyScope)cbScope.SelectedItem;
    private void CbAction_SelectedIndexChanged(object? _, EventArgs __) => UpdateAction();
    private void TxtPath_TextChanged(object? _, EventArgs __) => _profile.Path = txtPath.Text;
    private void CbHandled_CheckedChanged(object? _, EventArgs __) => _profile.IsHandled = cbHandled.Checked;
    private void SDelay_ValueChanged(object? _, int newValue) => _profile.Delay = newValue;
    private void BtnCollapse_Click(object? _, EventArgs __) => IsCollapsed = !_isCollapsed;
    private void BtnDelete_Click(object? _, EventArgs __) => _removeAction?.Invoke(_profile);
    private void TxtHotKeys_KeyDown(object? _, KeyEventArgs e) => e.SuppressKeyPress = true;
    private void TxtHotKeys_Enter(object? _, EventArgs __) => InitializeKeyboardHook();
    private void TxtHotKeys_Leave(object? _, EventArgs __)
    {
        DisposeKeyboardHook();

        // If the name is empty, set it to the hotkey.
        if (string.IsNullOrEmpty(txtName.Text))
            txtName.Text = txtHotKeys.Text;
    }
    private void LowLevelKeyboardHook_Down(object? _, KeyboardEventArgs e)
    {
        // Backspace removes the hotkey.
        if (e.Keys.Are(Key.Back))
        {
            Invoke(() => txtHotKeys.Text = string.Empty);
            _profile.HotKeys = null;
            return;
        }

        // Two or more keys and must have a modifier key. (CTRL, ALT, SHIFT, WIN)
        if (e.Keys.Values.Count < 2 || !e.Keys.Values.Any(k => k is Key.Ctrl or Key.Alt or Key.Shift or Key.LWin or Key.RWin))
            return;

        // Prevent the key from being handled by other applications.
        e.IsHandled = true;

        // Order the keys by modifier keys first, then by key.
        var keys = e.Keys.Values
            .OrderByDescending(key => key is Key.LWin or Key.RWin)
            .ThenBy(key => key)
            .ToArray();

        _profile.HotKeys = keys;
        Invoke(() => txtHotKeys.Text = string.Join(" + ", keys.Select(k => k.ToFixedString())));
    }

    // Methods
    private static void SetComboBoxDataSourceQuietly(ComboBox comboBox, object datasource, EventHandler eventHandler)
    {
        comboBox.SelectedIndexChanged -= eventHandler;
        comboBox.DataSource = datasource;
        comboBox.SelectedIndexChanged += eventHandler;
    }
    private void ToggleCollapse()
    {
        switch (_isCollapsed)
        {
            case true:
                Height = ExpandedHeight;
                btnCollapse.Text = @"ᐱ";
                break;
            default:
                Height = _collapsedHeight;
                btnCollapse.Text = @"ᐯ";
                break;
        }
    }
    private void UpdateControlsEnabledState()
    {
        var isEnabled = cbEnabled.Checked;
        _profile.IsEnabled = isEnabled;

        // Disable all controls if cbEnabled is unchecked, except for cbEnabled and btnCollapse.
        txtName.Enabled = isEnabled;
        txtHotKeys.Enabled = isEnabled;
        cbScope.Enabled = isEnabled;
        cbAction.Enabled = isEnabled;
        txtPath.Enabled = isEnabled;
        sDelay.Enabled = isEnabled;
        cbHandled.Enabled = isEnabled;
    }
    private void UpdateAction()
    {
        var selectedAction = (HotKeyAction)cbAction.SelectedItem;
        _profile.Action = selectedAction;

        switch (selectedAction)
        {
            case HotKeyAction.Open:
            {
                txtPath.Enabled = true;
                txtPath.Text = _profile.Path ?? string.Empty;
                break;
            }
            case HotKeyAction.Duplicate:
            {
                txtPath.Enabled = false;
                cbScope.SelectedIndex = cbScope.FindStringExact(nameof(HotkeyScope.FileExplorer));
                cbScope.Invalidate();
                break;
            }
        }
    }
    private void InitializeKeyboardHook()
    {
        DisposeKeyboardHook(false);
        _keyboardHookStarted?.Invoke();
        _lowLevelKeyboardHook = new LowLevelKeyboardHook { Handling = true };
        _lowLevelKeyboardHook.Down += LowLevelKeyboardHook_Down;
        _lowLevelKeyboardHook.Start();
    }
    private void DisposeKeyboardHook(bool inform = true)
    {
        if (_lowLevelKeyboardHook == default) return;
        _lowLevelKeyboardHook.Stop();
        _lowLevelKeyboardHook.Dispose();

        if (inform)
            _keyboardHookStopped?.Invoke();
    }
}