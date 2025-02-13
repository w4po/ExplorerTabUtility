using System;
using System.Threading;
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
    public event Action<HotKeyEventArgs>? OnHotKeyProfileTriggered;

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
        var handler = OnHotKeyProfileTriggered;
        if (handler == null) return;

        bool? isFileExplorerForeground = null;
        nint handle = 0;
        foreach (var profile in _hotkeyProfiles)
        {
            // Skip disabled, empty or mouse
            if (!profile.IsEnabled || profile.IsMouse || profile.HotKeys is null || profile.HotKeys.Length == 0)
                continue;

            // Skip if keys do not match
            if (!e.Keys.Are(profile.HotKeys)) continue;

            // Let's see if we need to check File Explorer
            if (profile.Scope == HotkeyScope.FileExplorer)
            {
                // Check if File Explorer is foreground (only once)
                isFileExplorerForeground ??= Helper.IsFileExplorerForeground(out handle);

                if (isFileExplorerForeground == false)
                {
                    handle = 0; // Reset handle if not File Explorer
                    continue;
                }
            }

            // Set handled value.
            e.IsHandled = profile.IsHandled;
            
            // Queue the hotkey trigger in a separate thread.
#if NET7_0_OR_GREATER
            ThreadPool.QueueUserWorkItem(static s => s.Handler.Invoke(new HotKeyEventArgs(s.Profile, s.Handle)),
                new State(handler, profile, handle), false);
#else
            ThreadPool.QueueUserWorkItem(static state =>
            {
                var s = (State)state!;
                s.Handler.Invoke(new HotKeyEventArgs(s.Profile, s.Handle));
            }, new State(handler, profile, handle));
#endif
        }
    }

    public void Dispose()
    {
        StopHook();
        _lowLevelKeyboardHook.Dispose();
    }

    private readonly struct State(Action<HotKeyEventArgs> handler, HotKeyProfile profile, nint handle)
    {
        public readonly Action<HotKeyEventArgs> Handler = handler;
        public readonly HotKeyProfile Profile = profile;
        public readonly nint Handle = handle;
    }
}