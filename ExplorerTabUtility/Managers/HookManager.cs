using System;
using System.Drawing;
using WindowsInput;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Hooks;
using System.Threading;

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
                _windowHook.DuplicateActiveTab(e.ForegroundWindow, e.Profile.IsAsTab);
                break;

            case HotKeyAction.ReopenClosed:
                _windowHook.ReopenClosedTab(e.Profile.IsAsTab, e.ForegroundWindow);
                break;

            case HotKeyAction.DetachTab:
                _windowHook.DetachCurrentTab(e.ForegroundWindow);
                break;

            case HotKeyAction.SetTargetWindow:
                _windowHook.SetTargetWindow(e.ForegroundWindow);
                break;

            case HotKeyAction.NavigateBack:
                NavigateBack(e.ForegroundWindow, e.MousePosition);
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
    private void NavigateBack(nint foregroundWindow,Point? mousePosition)
    {
        if (foregroundWindow == 0) return;

        if (mousePosition is not { } position)
            _windowHook.NavigateBack(foregroundWindow);
        else if (Helper.IsExplorerEmptySpace(position))
            Keyboard.Simulator.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.LEFT);
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
        _windowHook.Dispose();
    }
}