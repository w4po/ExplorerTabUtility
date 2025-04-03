using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Helpers;
using H.Hooks;

namespace ExplorerTabUtility.UI.Views;

// ReSharper disable once RedundantExtendsListEntry
public partial class HotKeyProfileControl : UserControl
{
    // Fields
    private Key _lastClickKey;
    private int _lastClickTime;
    private readonly HotKeyProfile _profile;
    private readonly Action<HotKeyProfile>? _removeAction;
    private readonly Action? _keybindingHookStarted;
    private readonly Action? _keybindingHookStopped;
    private LowLevelKeyboardHook? _lowLevelKeyboardHook;
    private LowLevelMouseHook? _lowLevelMouseHook;

    // Properties
    public new bool IsEnabled
    {
        get => CbEnabled.IsChecked ?? false;
        set => CbEnabled.IsChecked = value;
    }

    // Constructor
    public HotKeyProfileControl(HotKeyProfile profile, Action<HotKeyProfile>? removeAction = null, Action? keybindingHookStarted = null,
        Action? keybindingHookStopped = null)
    {
        InitializeComponent();
        Tag = profile;
        _profile = profile;
        _removeAction = removeAction;
        _keybindingHookStarted = keybindingHookStarted;
        _keybindingHookStopped = keybindingHookStopped;

        InitializeControls();
        SetupEventHandlers();
    }

    // Initialize controls with hot key profile data
    private void InitializeControls()
    {
        CbEnabled.IsChecked = _profile.IsEnabled;
        TxtName.Text = _profile.Name ?? string.Empty;

        if (_profile.HotKeys != null)
            TxtHotKeys.Text = _profile.HotKeys.HotKeysToString(_profile.IsDoubleClick);

        // Setup ComboBoxes
        CbScope.ItemsSource = Enum.GetValues(typeof(HotkeyScope));
        CbScope.SelectedItem = _profile.Scope;

        UpdateActionComboBox();
        CbAction.SelectedItem = _profile.Action;

        TxtPath.Text = _profile.Path ?? string.Empty;
        NDelay.Value = _profile.Delay;
        CbHandled.IsChecked = _profile.IsHandled;
        CbOpenAsTab.IsChecked = _profile.IsAsTab;

        UpdateControlsEnabledState();
    }

    private void SetupEventHandlers()
    {
        // Main controls
        CbEnabled.Checked += CbEnabled_CheckedChanged;
        CbEnabled.Unchecked += CbEnabled_CheckedChanged;
        TxtName.TextChanged += TxtName_TextChanged;
        TxtHotKeys.GotFocus += TxtHotKeys_Enter;
        TxtHotKeys.LostFocus += TxtHotKeys_Leave;
        TxtHotKeys.PreviewKeyDown += TxtHotKeys_KeyDown;
        CbScope.SelectionChanged += CbScope_SelectedIndexChanged;
        CbAction.SelectionChanged += CbAction_SelectedIndexChanged;
        BtnDelete.Click += BtnDelete_Click;

        // Additional controls
        TxtPath.TextChanged += TxtPath_TextChanged;
        NDelay.ValueChanged += NDelayValueChanged;
        CbHandled.Checked += CbHandled_CheckedChanged;
        CbHandled.Unchecked += CbHandled_CheckedChanged;
        CbOpenAsTab.Checked += CbOpenAsTab_CheckedChanged;
        CbOpenAsTab.Unchecked += CbOpenAsTab_CheckedChanged;
    }

    // Event handlers
    private void CbEnabled_CheckedChanged(object _, RoutedEventArgs __) => UpdateControlsEnabledState();
    private void TxtName_TextChanged(object _, TextChangedEventArgs __) => _profile.Name = TxtName.Text;

    private void CbScope_SelectedIndexChanged(object _, SelectionChangedEventArgs __)
    {
        _profile.Scope = (HotkeyScope)(CbScope.SelectedItem ?? 0);
        UpdateActionComboBox();
    }

    private void CbAction_SelectedIndexChanged(object _, SelectionChangedEventArgs __) => UpdateAction();
    private void TxtPath_TextChanged(object _, TextChangedEventArgs __) => _profile.Path = TxtPath.Text;
    private void NDelayValueChanged(object? _, RoutedPropertyChangedEventArgs<double> e) => _profile.Delay = (int)e.NewValue;
    private void CbHandled_CheckedChanged(object _, RoutedEventArgs __) => _profile.IsHandled = CbHandled.IsChecked ?? true;
    private void CbOpenAsTab_CheckedChanged(object _, RoutedEventArgs __) => _profile.IsAsTab = CbOpenAsTab.IsChecked ?? true;
    private void BtnDelete_Click(object _, RoutedEventArgs __) => _removeAction?.Invoke(_profile);
    private void TxtHotKeys_KeyDown(object sender, System.Windows.Input.KeyboardEventArgs e) => e.Handled = true;
    private void TxtHotKeys_Enter(object _, RoutedEventArgs __) => InitializeKeybindingHooks();

    private void TxtHotKeys_Leave(object _, RoutedEventArgs __)
    {
        DisposeKeybindingHooks();

        // If the name is empty, set it to the hotkey.
        if (string.IsNullOrWhiteSpace(TxtName.Text))
            TxtName.Text = TxtHotKeys.Text;
    }

    private void LowLevelHook_Down(object? _, KeyboardEventArgs e)
    {
        // Backspace removes the hotkey.
        if (e.Keys.Are(Key.Back))
        {
            Dispatcher.Invoke(() => TxtHotKeys.Text = string.Empty);
            _profile.HotKeys = null;
            return;
        }

        if (e.Keys.Are(Key.Tab))
        {
            e.IsHandled = true;
            MoveFocus(System.Windows.Input.FocusNavigationDirection.Right);
            return;
        }

        if (e.Keys.Are(Key.Shift, Key.Tab))
        {
            e.IsHandled = true;
            MoveFocus(System.Windows.Input.FocusNavigationDirection.Left);
            return;
        }

        var isMouse = false;
        var isDoubleClick = false;
        if (e.Keys.Values.Any(k => k is >= Key.MouseLeft and <= Key.MouseXButton2))
        {
            if (!IsMouseOverHotkeyTextBox())
                MoveFocus(null); // Remove focus / stop hooks
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

        Dispatcher.Invoke(() => TxtHotKeys.Text = keys.HotKeysToString(isDoubleClick));
    }

    private void MoveFocus(System.Windows.Input.FocusNavigationDirection? direction)
    {
        Dispatcher.BeginInvoke(() =>
        {
            var scope = System.Windows.Input.FocusManager.GetFocusScope(this);

            IInputElement? to = null;
            if (direction is { } dir)
            {
                var focusedElement = System.Windows.Input.FocusManager.GetFocusedElement(scope);
                if (focusedElement is FrameworkElement focused)
                    to = focused.PredictFocus(dir) as IInputElement;
            }

            System.Windows.Input.Keyboard.ClearFocus();
            System.Windows.Input.FocusManager.SetFocusedElement(scope, to);
        });
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
        return Dispatcher.Invoke(() =>
        {
            var mousePos = System.Windows.Input.Mouse.GetPosition(TxtHotKeys);
            return mousePos.X >= 0 && mousePos.X <= TxtHotKeys.ActualWidth &&
                   mousePos.Y >= 0 && mousePos.Y <= TxtHotKeys.ActualHeight;
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
    private void UpdateActionComboBox()
    {
        var allowedActions = GetAllowedActions(_profile.Scope);

        // Preserve the current action if it's allowed, otherwise use the first allowed action
        var currentAction = (HotKeyAction)(CbAction.SelectedItem ?? 0);
        var desiredAction = allowedActions.Contains(currentAction)
            ? currentAction
            : allowedActions.FirstOrDefault();

        CbAction.ItemsSource = allowedActions;
        CbAction.SelectedItem = desiredAction;
    }

    private void UpdateControlsEnabledState()
    {
        var isEnabled = CbEnabled.IsChecked ?? false;
        _profile.IsEnabled = isEnabled;

        // Disable all controls if cbEnabled is unchecked, except for cbEnabled and btnCollapse.
        ExpandedPanel.IsEnabled = isEnabled;
        TxtName.IsEnabled = isEnabled;
        TxtHotKeys.IsEnabled = isEnabled;
        CbScope.IsEnabled = isEnabled;
        CbAction.IsEnabled = isEnabled;
        TxtPath.IsEnabled = isEnabled && _profile.Action == HotKeyAction.Open;
        NDelay.IsEnabled = isEnabled;
        CbHandled.IsEnabled = isEnabled;
        CbOpenAsTab.IsEnabled = isEnabled && _profile.Action is
            HotKeyAction.Open or HotKeyAction.Duplicate or HotKeyAction.ReopenClosed;
    }

    private void UpdateAction()
    {
        var selectedAction = (HotKeyAction)(CbAction.SelectedItem ?? 0);
        _profile.Action = selectedAction;
        switch (selectedAction)
        {
            case HotKeyAction.Open:
                TxtPath.IsEnabled = CbEnabled.IsChecked ?? false;
                CbOpenAsTab.IsEnabled = CbEnabled.IsChecked ?? false;
                TxtPath.Text = _profile.Path ?? string.Empty;
                break;
            case HotKeyAction.Duplicate:
            case HotKeyAction.ReopenClosed:
                TxtPath.IsEnabled = false;
                CbOpenAsTab.IsEnabled = CbEnabled.IsChecked ?? false;
                break;
            default:
                TxtPath.IsEnabled = false;
                CbOpenAsTab.IsEnabled = false;
                break;
        }
        
        // If the name is empty or is an exact match of an action, set it to the hotkey.
        var isExactMatch = Enum.GetNames(typeof(HotKeyAction)).Any(a => a == TxtName.Text);
        if (string.IsNullOrWhiteSpace(TxtName.Text) || isExactMatch)
            TxtName.Text = selectedAction.ToString();
    }

    private static HotKeyAction[] GetAllowedActions(HotkeyScope scope)
    {
        return scope switch
        {
            HotkeyScope.Global =>
            [
                HotKeyAction.Open,
                HotKeyAction.TabSearch,
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
            _lowLevelKeyboardHook = null;
        }

        if (_lowLevelMouseHook != null)
        {
            _lowLevelMouseHook.Stop();
            _lowLevelMouseHook.Dispose();
            _lowLevelMouseHook = null;
        }

        if (inform)
            _keybindingHookStopped?.Invoke();
    }
}