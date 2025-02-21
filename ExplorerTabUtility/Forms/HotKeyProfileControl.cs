using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Helpers;
using H.Hooks;

namespace ExplorerTabUtility.Forms;

public partial class HotKeyProfileControl : UserControl
{
    // Fields
    private Key _lastClickKey;
    private int _lastClickTime;
    private bool _isCollapsed;
    private readonly int _collapsedHeight;
    private readonly HotKeyProfile _profile;
    private readonly Action<HotKeyProfile>? _removeAction;
    private readonly Action? _keybindingHookStarted;
    private readonly Action? _keybindingHookStopped;
    private LowLevelKeyboardHook? _lowLevelKeyboardHook;
    private LowLevelMouseHook? _lowLevelMouseHook;
    // Constants
    private const int ExpandedHeight = 76;
    // Properties
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool IsEnabled
    {
        get => cbEnabled.Checked;
        set
        {
            if (cbEnabled.Checked == value) return;
            cbEnabled.Checked = value;
        }
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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
    public HotKeyProfileControl(HotKeyProfile profile, Action<HotKeyProfile>? removeAction = null, Action? keybindingHookStarted = null, Action? keybindingHookStopped = null)
    {
        InitializeComponent();
        Tag = profile;
        _profile = profile;
        _collapsedHeight = Height;
        _removeAction = removeAction;
        _keybindingHookStarted = keybindingHookStarted;
        _keybindingHookStopped = keybindingHookStopped;
        InitializeControls();
    }
    // Initialize controls with hot key profile data
    private void InitializeControls()
    {
        cbEnabled.Checked = _profile.IsEnabled;
        txtName.Text = _profile.Name ?? string.Empty;

        if (_profile.HotKeys != null)
            txtHotKeys.Text = _profile.HotKeys.HotKeysToString(_profile.IsDoubleClick);

        SetComboBoxDataSourceQuietly(cbScope, Enum.GetValues(typeof(HotkeyScope)), CbScope_SelectedIndexChanged);
        SetComboBoxDataSourceQuietly(cbAction, GetAllowedActions(_profile.Scope), CbAction_SelectedIndexChanged);
        cbScope.SelectedItem = _profile.Scope;
        cbAction.SelectedItem = _profile.Action;
        txtPath.Text = _profile.Path ?? string.Empty;
        sDelay.Value = _profile.Delay;
        cbHandled.Checked = _profile.IsHandled;
        cbOpenAsTab.Checked = _profile.IsAsTab;
    }

    // Event handlers
    private void ComboBox_DropDownClosed(object? _, EventArgs __) => toolTip.Hide(cbAction);
    private void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var cb = sender as ComboBox;
        if (cb!.Items[e.Index] is not Enum item) return;

        var text = Helper.GetEnumDescription(item);

        // Show tooltip only if focused AND visible within ComboBox viewport
        if (e.State.HasFlag(DrawItemState.Focus) &&
            e.Bounds.Top >= 0 && e.Bounds.Bottom <= cb.DropDownHeight)
        {
            toolTip.Show(text, cb, e.Bounds.Right, e.Bounds.Bottom, 5000);
        }
        else
            toolTip.Hide(cb);
    }
    private void CbEnabled_CheckedChanged(object? _, EventArgs __) => UpdateControlsEnabledState();
    private void TxtName_TextChanged(object? _, EventArgs __) => _profile.Name = txtName.Text;
    private void CbScope_SelectedIndexChanged(object? _, EventArgs __)
    {
        _profile.Scope = (HotkeyScope)(cbScope.SelectedItem ?? 0);

        var allowedActions = GetAllowedActions(_profile.Scope);

        // Preserve the current action if it's allowed, otherwise use the first allowed action
        var currentAction = (HotKeyAction)(cbAction.SelectedItem ?? 0);
        var desiredAction = allowedActions.Contains(currentAction)
            ? currentAction
            : allowedActions.FirstOrDefault();

        // Update the ComboBox data source
        SetComboBoxDataSourceQuietly(cbAction, allowedActions, CbAction_SelectedIndexChanged);

        cbAction.SelectedItem = desiredAction;
    }
    private void CbAction_SelectedIndexChanged(object? _, EventArgs __) => UpdateAction();
    private void TxtPath_TextChanged(object? _, EventArgs __) => _profile.Path = txtPath.Text;
    private void CbHandled_CheckedChanged(object? _, EventArgs __) => _profile.IsHandled = cbHandled.Checked;
    private void CbOpenAsTab_CheckedChanged(object? _, EventArgs __) => _profile.IsAsTab = cbOpenAsTab.Checked;
    private void SDelay_ValueChanged(object? _, int newValue) => _profile.Delay = newValue;
    private void BtnCollapse_Click(object? _, EventArgs __) => IsCollapsed = !_isCollapsed;
    private void BtnDelete_Click(object? _, EventArgs __) => _removeAction?.Invoke(_profile);
    private void TxtHotKeys_KeyDown(object? _, KeyEventArgs e) => e.SuppressKeyPress = true;
    private void TxtHotKeys_Enter(object? _, EventArgs __) => InitializeKeybindingHooks();
    private void TxtHotKeys_Leave(object? _, EventArgs __)
    {
        DisposeKeybindingHooks();

        // If the name is empty, set it to the hotkey.
        if (string.IsNullOrEmpty(txtName.Text))
            txtName.Text = txtHotKeys.Text;
    }
    private void LowLevelHook_Down(object? _, KeyboardEventArgs e)
    {
        // Backspace removes the hotkey.
        if (e.Keys.Are(Key.Back))
        {
            Invoke(() => txtHotKeys.Text = string.Empty);
            _profile.HotKeys = null;
            return;
        }

        var isMouse = false;
        var isDoubleClick = false;
        if (e.Keys.Values.Any(k => k is >= Key.MouseLeft and <= Key.MouseXButton2))
        {
            if (!IsMouseOverHotkeyTextBox())
                Invoke(void () => ActiveControl = null); // Remove focus / stop hooks
            else
            {
                isMouse = true;
                isDoubleClick = IsDoubleClick(e.CurrentKey);
            }
        }

        if (!isMouse && !IsAllowedKeys(e.Keys.Values)) return;

        // Prevent the key from being handled by other applications.
        e.IsHandled = true;

        // Order the keys by modifier keys first, then by key.
        var keys = e.Keys.Values
            .OrderByDescending(key => key is Key.LWin or Key.RWin)
            .ThenBy(key => key)
            .ToArray();

        _profile.HotKeys = keys;
        _profile.IsMouse = isMouse;
        _profile.IsDoubleClick = isDoubleClick;

        Invoke(() => txtHotKeys.Text = keys.HotKeysToString(isDoubleClick));
    }
    private static bool IsAllowedKeys(IReadOnlyCollection<Key> keys)
    {
        // CTRL, ALT, SHIFT, WIN
        if (keys.Any(k => k is Key.Ctrl or Key.Alt or Key.Shift or Key.LWin or Key.RWin))
            return true;

        // F1 to F23
        if (keys.Any(k => k is >= Key.F1 and <= Key.F23))
            return true;

        // *, +, -, /
        if (keys.Any(k => k is Key.Multiply or Key.Add or Key.Subtract or Key.Divide))
            return true;

        // PageUp, PageDown, Print, PrintScreen, Insert, Delete
        if (keys.Any(k => k is Key.PageUp or Key.PageDown or Key.Print or Key.PrintScreen or Key.Insert or Key.Delete))
            return true;

        return false;
    }
    private bool IsMouseOverHotkeyTextBox()
    {
        return (bool)Invoke(() =>
        {
            // Convert the global mouse position to the TextBox's client coordinates
            var mousePositionInTextBox = txtHotKeys.PointToClient(Cursor.Position);

            // Check if this point is within the TextBox's client rectangle
            return txtHotKeys.ClientRectangle.Contains(mousePositionInTextBox);
        });
    }

    private bool IsDoubleClick(Key currentKey)
    {
        var isDoubleClick = false;

        var now = Environment.TickCount;
        if (now - _lastClickTime < 500 && _lastClickKey == currentKey)
            isDoubleClick = true;

        _lastClickTime = now;
        _lastClickKey = currentKey;
        return isDoubleClick;
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
        var selectedAction = (HotKeyAction)(cbAction.SelectedItem ?? 0);
        _profile.Action = selectedAction;

        switch (selectedAction)
        {
            case HotKeyAction.Open:
                txtPath.Enabled = true;
                cbOpenAsTab.Enabled = true;
                txtPath.Text = _profile.Path ?? string.Empty;
                break;
            case HotKeyAction.Duplicate:
            case HotKeyAction.ReopenClosed:
                txtPath.Enabled = false;
                cbOpenAsTab.Enabled = true;
                break;
            default:
                txtPath.Enabled = false;
                cbOpenAsTab.Enabled = false;
                break;
        }
    }
    private static HotKeyAction[] GetAllowedActions(HotkeyScope scope)
    {
        return scope switch
        {
            HotkeyScope.Global =>
            [
                HotKeyAction.Open,
                HotKeyAction.ToggleWinHook,
                HotKeyAction.ToggleReuseTabs,
                HotKeyAction.ToggleVisibility,
                HotKeyAction.SnapRight,
                HotKeyAction.SnapLeft,
                HotKeyAction.SnapUp,
                HotKeyAction.SnapDown
            ],
            _ => Enum.GetValues(typeof(HotKeyAction))
                .OfType<HotKeyAction>()
                .ToArray()
        };
    }


    private void InitializeKeybindingHooks()
    {
        DisposeKeybindingHooks(false);
        _keybindingHookStarted?.Invoke();

        _lowLevelKeyboardHook = new LowLevelKeyboardHook { Handling = true };
        _lowLevelKeyboardHook.Down += LowLevelHook_Down;
        _lowLevelKeyboardHook.Start();

        _lowLevelMouseHook = new LowLevelMouseHook { AddKeyboardKeys = true };
        _lowLevelMouseHook.Down += LowLevelHook_Down;
        _lowLevelMouseHook.Start();
    }
    private void DisposeKeybindingHooks(bool inform = true)
    {
        if (_lowLevelKeyboardHook != null)
        {
            _lowLevelKeyboardHook.Stop();
            _lowLevelKeyboardHook.Dispose();
        }
        if (_lowLevelMouseHook != null)
        {
            _lowLevelMouseHook.Stop();
            _lowLevelMouseHook.Dispose();
        }

        if (inform)
            _keybindingHookStopped?.Invoke();
    }
}