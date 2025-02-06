using System;
using System.Drawing;
using System.Collections.Generic;
using WindowsInput;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Hooks;

namespace ExplorerTabUtility.Managers;

public sealed class HookManager
{
    private readonly Mouse _mouseHook;
    private readonly Keyboard _keyboardHook;
    private readonly ExplorerWatcher _windowHook;
    public bool IsMouseHookStarted => _mouseHook.IsHookActive;
    public bool IsKeyboardHookStarted => _keyboardHook.IsHookActive;
    public bool IsWindowHookStarted => _windowHook.IsHookActive;
    public event Action? OnVisibilityToggled;
    public event Action? OnWindowHookToggled;
    public event Action? OnReuseTabsToggled;

    public HookManager(IReadOnlyCollection<HotKeyProfile> hotKeyProfiles)
    {
        _windowHook = new ExplorerWatcher();
        _mouseHook = new Mouse(hotKeyProfiles);
        _keyboardHook = new Keyboard(hotKeyProfiles);
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
            case HotKeyAction.Open: await _windowHook.Open(e.Profile.Path, e.ForegroundWindow, e.Profile.Delay); break;
            case HotKeyAction.Duplicate: _windowHook.DuplicateActiveTab(e.ForegroundWindow); break;
            case HotKeyAction.ReopenClosed: _windowHook.ReopenClosedTab(e.ForegroundWindow); break;
            case HotKeyAction.SetTargetWindow: _windowHook.SetTargetWindow(e.ForegroundWindow); break;
            case HotKeyAction.ToggleVisibility: OnVisibilityToggled?.Invoke();  break;
            case HotKeyAction.ToggleWinHook: OnWindowHookToggled?.Invoke();  break;
            case HotKeyAction.ToggleReuseTabs: OnReuseTabsToggled?.Invoke();  break;
            case HotKeyAction.NavigateBack: NavigateBack(e.ForegroundWindow, e.MousePosition); break;
            default: throw new ArgumentOutOfRangeException(nameof(e.Profile), e.Profile.Action, @"Invalid profile action");
        }
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