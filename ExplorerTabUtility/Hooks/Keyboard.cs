using WindowsInput;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;
using System.Threading;

namespace ExplorerTabUtility.Hooks;

public class Keyboard : IDisposable
{
    private nint _hookId = 0;
    private nint _user32LibraryHandle = 0;
    private bool _isWinKeyDown;
    private HookProc? _keyboardHookCallback; // We have to keep a reference because of GC
    private readonly Func<Window, Task> _onNewWindow;
    private static readonly IKeyboardSimulator KeyboardSimulator = new InputSimulator().Keyboard;

    public Keyboard(Func<Window, Task> onNewWindow)
    {
        _onNewWindow = onNewWindow;
    }

    public void StartHook()
    {
        _keyboardHookCallback = KeyboardHookCallback;
        _user32LibraryHandle = WinApi.LoadLibrary("User32");
        _hookId = WinApi.SetWindowsHookEx(WinHookType.WH_KEYBOARD_LL, _keyboardHookCallback, _user32LibraryHandle, 0);
    }

    public void StopHook()
    {
        Dispose();
    }

    private nint KeyboardHookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0)
            return WinApi.CallNextHookEx(_hookId, nCode, wParam, lParam);

        // Read key
        var vkCode = Marshal.ReadInt32(lParam);

        // Windows key
        if (vkCode == WinApi.VK_WIN)
            _isWinKeyDown = wParam == WinApi.WM_KEYDOWN; //DOWN or UP

        if (!_isWinKeyDown || vkCode != WinApi.VK_E || wParam != WinApi.WM_KEYDOWN)
            return WinApi.CallNextHookEx(_hookId, nCode, wParam, lParam);

        // No Explorer windows, Continue with normal flow.
        if (!WinApi.FindAllWindowsEx().Take(1).Any())
            return WinApi.CallNextHookEx(_hookId, nCode, wParam, lParam);

        // It is better not to wait for the invocation, otherwise the normal flow might open a new window
        Task.Run(() => _onNewWindow.Invoke(new Window(string.Empty)));

        // Return dummy value to prevent normal flow.
        return 1;
    }

    public static void AddNewTab(nint windowHandle)
    {
        // Restore the window to foreground.
        WinApi.RestoreWindowToForeground(windowHandle);

        // Give the focus to the folder view.
        WinApi.PostMessage(windowHandle, WinApi.WM_SETFOCUS, 0, 0);

        // Send CTRL + T
        KeyboardSimulator
            .Sleep(300)
            .ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_T);
    }
    public static void Navigate(nint windowHandle, nint tabHandle, string location)
    {
        // Restore the window to foreground.
        WinApi.RestoreWindowToForeground(windowHandle);

        // Give the keyboard focus to the tab.
        WinApi.PostMessage(tabHandle, WinApi.WM_SETFOCUS, 0, 0);

        // Send CTRL + L to activate the address bar
        KeyboardSimulator
            .Sleep(300)
            .ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_L);

        // Type the location.
        KeyboardSimulator
            .Sleep(300)
            .TextEntry(location)
            .Sleep(250 + location.Length * 5) // Longer locations require longer wait time.
            .KeyPress(VirtualKeyCode.RETURN); // Press Enter

        // Do in the background
        Task.Run(async () =>
        {
            // for ~1250 Milliseconds (25 * 50)
            for (var i = 0; i < 25; i++)
            {
                await Task.Delay(50);

                var popupHandle = WinApi.GetWindow(windowHandle, WinApi.GW_ENABLEDPOPUP);

                // If the suggestion popup is not visible, continue.
                if (popupHandle == 0) continue;

                // Hide the suggestion popup.
                WinApi.ShowWindow(popupHandle, WinApi.SW_HIDE);
            }
        });
    }
    public static void SelectItems(nint tabHandle, ICollection<string> names)
    {
        // Restore the window to foreground.
        WinApi.RestoreWindowToForeground(tabHandle);

        Thread.Sleep(500);

        // Type the first name.
        KeyboardSimulator.TextEntry(names.First());
    }

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            WinApi.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        _keyboardHookCallback = null;
        if (_user32LibraryHandle == IntPtr.Zero) return;

        // reduces reference to library by 1.
        WinApi.FreeLibrary(_user32LibraryHandle);
        _user32LibraryHandle = IntPtr.Zero;
    }
    ~Keyboard()
    {
        Dispose();
    }
}