using System;
using System.Collections.Generic;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Hooks;

namespace ExplorerTabUtility.Managers;

public class HookManager(
    IReadOnlyCollection<HotKeyProfile> hotKeyProfiles,
    Action<HotKeyProfile> onHotKeyProfileTriggered,
    Action<Window, bool> onNewWindow)
{
    private readonly IHook _keyboardHook = new Keyboard(hotKeyProfiles, onHotKeyProfileTriggered);
    private readonly IHook _windowHook = new Shell32(onNewWindow);

    public bool IsKeyboardHookStarted => _keyboardHook.IsHookActive;
    public bool IsWindowHookStarted => _windowHook.IsHookActive;

    public void StartKeyboardHook() => ChangeHookStatus(_keyboardHook, true);
    public void StopKeyboardHook() => ChangeHookStatus(_keyboardHook, false);
    public void StartWindowHook() => ChangeHookStatus(_windowHook, true);
    public void StopWindowHook() => ChangeHookStatus(_windowHook, false);
    
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