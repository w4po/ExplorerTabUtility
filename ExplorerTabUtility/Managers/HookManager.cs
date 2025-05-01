using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.UI.Views;

namespace ExplorerTabUtility.Managers;

public sealed class HookManager
{
    private readonly Mouse _mouseHook;
    private readonly Keyboard _keyboardHook;
    private readonly ExplorerWatcher _windowHook;
    private readonly SynchronizationContext _syncContext;
    public event Action? OnVisibilityToggled;
    public event Action? OnWindowHookToggled;
    public event Action? OnReuseTabsToggled;
    public event Action? OnShellInitialized;

    public HookManager(ProfileManager profileManager)
    {
        _syncContext = SynchronizationContext.Current!;

        _windowHook = new ExplorerWatcher();
        _mouseHook = new Mouse(profileManager.GetProfiles());
        _keyboardHook = new Keyboard(profileManager.GetProfiles());

        profileManager.KeybindingsHookStarted += KeybindingStarted;
        profileManager.KeybindingsHookStopped += KeybindingStopped;
        _keyboardHook.OnHotKeyProfileTriggered += OnHotKeyProfileTriggered;
        _mouseHook.OnHotKeyProfileTriggered += OnHotKeyProfileTriggered;
        _windowHook.OnShellInitialized += () => OnShellInitialized?.Invoke();
        System.Windows.Application.Current.SessionEnding += (_, _) => Dispose();
    }

    public void StartMouseHook() => ChangeHookStatus(_mouseHook, true);
    public void StopMouseHook() => ChangeHookStatus(_mouseHook, false);
    public void StartKeyboardHook() => ChangeHookStatus(_keyboardHook, true);
    public void StopKeyboardHook() => ChangeHookStatus(_keyboardHook, false);
    public void StartWindowHook() => ChangeHookStatus(_windowHook, true);
    public void StopWindowHook() => ChangeHookStatus(_windowHook, false);
    public void SetReuseTabs(bool value) => _windowHook.SetReuseTabs(value);

    private async void OnHotKeyProfileTriggered(HotKeyEventArgs e)
    {
        switch (e.Profile.Action)
        {
            case HotKeyAction.Open:
                await _windowHook.Open(e.Profile.Path, e.Profile.IsAsTab, e.ForegroundWindow, e.Profile.Delay);
                break;

            case HotKeyAction.Duplicate:
                await _windowHook.DuplicateActiveTab(e.ForegroundWindow, e.Profile.IsAsTab);
                break;

            case HotKeyAction.ReopenClosed:
                await _windowHook.ReopenClosedTab(e.Profile.IsAsTab, e.ForegroundWindow);
                break;

            case HotKeyAction.DetachTab:
                await _windowHook.DetachCurrentTab(e.ForegroundWindow);
                break;

            case HotKeyAction.SetTargetWindow:
                _windowHook.SetTargetWindow(e.ForegroundWindow);
                break;

            case HotKeyAction.NavigateBack:
                NavigateBackForward(e.ForegroundWindow, e.MousePosition, isForward: false);
                break;

            case HotKeyAction.NavigateUp:
                NavigateUp(e.ForegroundWindow, e.MousePosition);
                break;
            
            case HotKeyAction.NavigateForward:
                NavigateBackForward(e.ForegroundWindow, e.MousePosition, isForward: true);
                break;

            case HotKeyAction.ToggleReuseTabs:
                _syncContext.Post(_ => OnReuseTabsToggled?.Invoke(), null);
                break;

            case HotKeyAction.ToggleWinHook:
                _syncContext.Post(_ => OnWindowHookToggled?.Invoke(), null);
                break;

            case HotKeyAction.ToggleVisibility:
                _syncContext.Post(_ => OnVisibilityToggled?.Invoke(), null);
                break;

            case HotKeyAction.TabSearch:
                _syncContext.Post(_ => new TabSearchPopup(_windowHook).Show(), null);
                break;

            case HotKeyAction.SnapRight:
            case HotKeyAction.SnapLeft:
            case HotKeyAction.SnapUp:
            case HotKeyAction.SnapDown:
                await SnapForegroundWindow(e.Profile.Action, e.Profile.Delay);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(e.Profile.Action),
                    e.Profile.Action,
                    @"Invalid profile action");
        }
    }
    private void KeybindingStarted()
    {
        StopMouseHook();
        StopKeyboardHook();
    }
    private void KeybindingStopped()
    {
        if (SettingsManager.IsMouseHookActive) StartMouseHook();
        if (SettingsManager.IsKeyboardHookActive) StartKeyboardHook();
    }
    private void NavigateBackForward(nint foregroundWindow, Point? mousePosition, bool isForward)
    {
        if (foregroundWindow == 0) return;

        if (mousePosition is not { } position)
            _windowHook.NavigateBackForward(foregroundWindow, isForward);
        else if (Helper.IsExplorerEmptySpace(position))
            KeyboardSimulator.ModifiedKeyStroke(VirtualKey.Alt, isForward ? VirtualKey.Right : VirtualKey.Left);
    }
    private void NavigateUp(nint foregroundWindow, Point? mousePosition)
    {
        if (foregroundWindow == 0) return;

        if (mousePosition is not { } position)
            KeyboardSimulator.ModifiedKeyStroke(VirtualKey.Alt, VirtualKey.Up);
        else if (Helper.IsExplorerEmptySpace(position))
            KeyboardSimulator.ModifiedKeyStroke(VirtualKey.Alt, VirtualKey.Up);
    }

    private async Task SnapForegroundWindow(HotKeyAction direction, int delay = 0)
    {
        var snapKey = GetSnapKey(direction);
        if (snapKey == VirtualKey.None) return;

        if (delay > 0)
            await Task.Delay(delay);

        var inputs = new List<INPUT>();
        // Remove any currently pressed modifiers (Ctrl, Shift, Alt, Win)
        inputs.AddUpEventsForCurrentlyPressedModifiers();

        // Press Windows key
        inputs.AddKeyDown(VirtualKey.LWin);

        // For up and down press Alt key
        if (snapKey is VirtualKey.Up or VirtualKey.Down)
            inputs.AddKeyDown(VirtualKey.Alt);

        // Press the snap key
        inputs.AddKeyPress(snapKey);

        // For up and down release Alt key
        if (snapKey is VirtualKey.Up or VirtualKey.Down)
            inputs.AddKeyUp(VirtualKey.Alt);

        // Release Windows key
        inputs.AddKeyUp(VirtualKey.LWin);

        // Re-add any currently pressed modifiers
        inputs.AddDownEventsForCurrentlyPressedModifiers();

        KeyboardSimulator.SendInputs(inputs.ToArray());
    }
    private static VirtualKey GetSnapKey(HotKeyAction direction)
    {
        return direction switch
        {
            HotKeyAction.SnapRight => VirtualKey.Right,
            HotKeyAction.SnapLeft => VirtualKey.Left,
            HotKeyAction.SnapUp => VirtualKey.Up,
            HotKeyAction.SnapDown => VirtualKey.Down,
            _ => VirtualKey.None
        };
    }

    private static void ChangeHookStatus(IHook hook, bool isActive)
    {
        if (hook.IsHookActive == isActive) return;

        if (isActive)
            hook.StartHook();
        else
            hook.StopHook();
    }

    public void Dispose()
    {
        _keyboardHook.Dispose();
        _mouseHook.Dispose();
        _windowHook.Dispose();
    }
}