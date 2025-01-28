using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using H.Hooks;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Hooks;

public sealed class Keyboard : IHook
{
    private readonly LowLevelKeyboardHook _lowLevelKeyboardHook;
    private readonly IReadOnlyCollection<HotKeyProfile> _hotkeyProfiles;
    public bool IsHookActive => _lowLevelKeyboardHook.IsStarted;
    public event Action<HotKeyProfile, nint>? OnHotKeyProfileTriggered;

    public Keyboard(IReadOnlyCollection<HotKeyProfile> hotkeyProfiles)
    {
        _hotkeyProfiles = hotkeyProfiles;
        _lowLevelKeyboardHook = new LowLevelKeyboardHook { Handling = true };
        _lowLevelKeyboardHook.Down += LowLevelKeyboardHook_Down;
    }

    public void StartHook() => _lowLevelKeyboardHook.Start();
    public void StopHook() => _lowLevelKeyboardHook.Stop();

    private void LowLevelKeyboardHook_Down(object? sender, KeyboardEventArgs e)
    {
        if (OnHotKeyProfileTriggered == null) return;

        var isFileExplorerForeground = Helper.IsFileExplorerForeground(out var handle);

        foreach (var profile in _hotkeyProfiles)
        {
            // Skip if the profile is disabled or if it doesn't have any hotkeys.
            if (!profile.IsEnabled || profile.HotKeys is null || profile.HotKeys.Length == 0) continue;

            // Skip if the profile is for File Explorer but File Explorer is not the foreground window.
            if (profile.Scope == HotkeyScope.FileExplorer && !isFileExplorerForeground) continue;

            // Skip if the hotkeys don't match.
            if (!e.Keys.Are(profile.HotKeys)) continue;

            // Set handled value.
            e.IsHandled = profile.IsHandled;

            // Invoke the profile action in the background in order for `IsHandled` to successfully prevent further processing.
            Task.Run(() => OnHotKeyProfileTriggered.Invoke(profile, isFileExplorerForeground ? handle : 0));
        }
    }

    public void Dispose()
    {
        StopHook();
        _lowLevelKeyboardHook.Dispose();
    }
}