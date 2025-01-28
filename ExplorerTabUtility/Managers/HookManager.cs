using System;
using System.Collections.Generic;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Hooks;
using System.Threading.Tasks;

namespace ExplorerTabUtility.Managers;

public sealed class HookManager
{
    private readonly Keyboard _keyboardHook;
    private readonly ExplorerWatcher _windowHook;
    public bool IsKeyboardHookStarted => _keyboardHook.IsHookActive;
    public bool IsWindowHookStarted => _windowHook.IsHookActive;

    public HookManager(IReadOnlyCollection<HotKeyProfile> hotKeyProfiles)
    {
        _windowHook = new ExplorerWatcher();
        _keyboardHook = new Keyboard(hotKeyProfiles);
        _keyboardHook.OnHotKeyProfileTriggered += OnHotKeyProfileTriggered;
    }

    public void StartKeyboardHook() => ChangeHookStatus(_keyboardHook, true);
    public void StopKeyboardHook() => ChangeHookStatus(_keyboardHook, false);
    public void StartWindowHook() => ChangeHookStatus(_windowHook, true);
    public void StopWindowHook() => ChangeHookStatus(_windowHook, false);

    public async void OnHotKeyProfileTriggered(HotKeyProfile profile, nint foregroundWindow = 0)
    {
        switch (profile.Action)
        {
            case HotKeyAction.Open:
            {
                if (profile.Delay > 0)
                    await Task.Delay(profile.Delay).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(profile.Path))
                    _windowHook.RequestToOpenNewTab(foregroundWindow, bringToFront: true);
                else
                    _windowHook.OpenNewTab(foregroundWindow, profile.Path!);

                break;
            }
            case HotKeyAction.Duplicate: _windowHook.DuplicateActiveTab(foregroundWindow); break;
            case HotKeyAction.ReopenClosed: _windowHook.ReopenClosedTab(foregroundWindow); break;
            default: throw new ArgumentOutOfRangeException(nameof(profile), profile.Action, @"Invalid profile action");
        }
    }

    public void Dispose()
    {
        _keyboardHook.Dispose();
        _windowHook.Dispose();
    }
    private static void ChangeHookStatus(IHook hook, bool isActive)
    {
        if (hook.IsHookActive == isActive) return;
        
        if (isActive)
            hook.StartHook();
        else
            hook.StopHook();
    }
}