using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ExplorerTabUtility.Managers;
using WindowsInput;
using H.Hooks;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Hooks;

public class Keyboard : IHook
{
    private readonly LowLevelKeyboardHook _lowLevelKeyboardHook;
    private readonly IReadOnlyCollection<HotKeyProfile> _hotkeyProfiles;
    private readonly Action<HotKeyProfile> _onHotKeyProfileTriggered;
    private static readonly IKeyboardSimulator KeyboardSimulator = new InputSimulator().Keyboard;
    public bool IsHookActive => _lowLevelKeyboardHook.IsStarted;

    public Keyboard(IReadOnlyCollection<HotKeyProfile> hotkeyProfiles, Action<HotKeyProfile> onHotKeyProfileTriggered)
    {
        _hotkeyProfiles = hotkeyProfiles;
        _onHotKeyProfileTriggered = onHotKeyProfileTriggered;
        _lowLevelKeyboardHook = new LowLevelKeyboardHook { Handling = true };
        _lowLevelKeyboardHook.Down += LowLevelKeyboardHook_Down;
    }

    public void StartHook() => _lowLevelKeyboardHook.Start();
    public void StopHook() => _lowLevelKeyboardHook.Stop();

    private void LowLevelKeyboardHook_Down(object? sender, KeyboardEventArgs e)
    {
        var isFileExplorerForeground = WinApi.IsFileExplorerForeground(out _);

        foreach (var profile in _hotkeyProfiles)
        {
            // Skip if the profile is disabled or if it doesn't have any hotkeys.
            if (!profile.IsEnabled || profile.HotKeys?.Any() != true) continue;

            // Skip if the profile is for File Explorer but File Explorer is not the foreground window.
            if (profile.Scope == HotkeyScope.FileExplorer && !isFileExplorerForeground) continue;

            // Skip if the hotkeys don't match.
            if (!e.Keys.Are(profile.HotKeys)) continue;

            // Set handled value.
            e.IsHandled = profile.IsHandled;

            // Invoke the profile action in the background in order for `IsHandled` to successfully prevent further processing.
            Task.Run(() => _onHotKeyProfileTriggered(profile));
        }
    }
    public static async Task<string?> GetCurrentTabLocationAsync(nint windowHandle, bool restoreToForeground = true)
    {
        // Restore the window to foreground.
        if (restoreToForeground)
        {
            WinApi.RestoreWindowToForeground(windowHandle);
            await Task.Delay(350).ConfigureAwait(false);
        }

        // Send CTRL + L to activate the address bar
        KeyboardSimulator.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_L);
        await Task.Delay(150).ConfigureAwait(false);

        // Store the current clipboard data
        var backup = ClipboardManager.GetClipboardData();

        // Clear the clipboard.
        ClipboardManager.ClearClipboard();

        // Send CTRL + C to copy the address location.
        KeyboardSimulator.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

        // Get the text from the clipboard.
        var addressLocation = await Helper.DoUntilConditionAsync(
            action: ClipboardManager.GetClipboardText,
            predicate: l => !string.IsNullOrWhiteSpace(l))
            .ConfigureAwait(false);
        
        // Give the focus to the window to close the address bar.
        WinApi.PostMessage(windowHandle, WinApi.WM_SETFOCUS, 0, 0);
        
        // Restore the previous clipboard data.
        ClipboardManager.SetClipboardData(backup);

        return addressLocation;
    }

    public static async Task AddNewTabAsync(nint windowHandle)
    {
        // Restore the window to foreground.
        WinApi.RestoreWindowToForeground(windowHandle);

        await Task.Delay(200).ConfigureAwait(false);

        // Send CTRL + T
        KeyboardSimulator.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_T);
    }
    public static async Task NavigateAsync(nint windowHandle, string location)
    {
        // Restore the window to foreground.
        WinApi.RestoreWindowToForeground(windowHandle);

        await Task.Delay(300).ConfigureAwait(false);

        // Send CTRL + L to activate the address bar
        KeyboardSimulator.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_L);

        await Task.Delay(300).ConfigureAwait(false);

        // Type the location.
        KeyboardSimulator.TextEntry(location);

        // Longer locations require longer wait time.
        await Task.Delay(270 + location.Length * 5).ConfigureAwait(false);

        // Press Enter
        KeyboardSimulator.KeyPress(VirtualKeyCode.RETURN);
    }
    public static async Task SelectItemsAsync(nint tabHandle, ICollection<string> names)
    {
        // Restore the window to foreground.
        WinApi.RestoreWindowToForeground(tabHandle);

        await Task.Delay(500).ConfigureAwait(false);

        // Type the first name.
        KeyboardSimulator.TextEntry(names.First());
    }

    public void Dispose()
    {
        StopHook();
        _lowLevelKeyboardHook.Dispose();
    }

    ~Keyboard() => Dispose();
}