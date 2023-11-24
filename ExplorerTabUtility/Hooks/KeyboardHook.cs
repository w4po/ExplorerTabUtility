using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;

namespace ExplorerTabUtility.Hooks;

public class KeyboardHook : IDisposable
{
    private IntPtr _hookId = IntPtr.Zero;
    private IntPtr _user32LibraryHandle = IntPtr.Zero;
    private bool _isWinKeyDown;
    private HookProc? _keyboardHookCallback; // We have to keep a reference because of GC
    private readonly Func<Window, Task> _onNewWindow;

    public KeyboardHook(Func<Window, Task> onNewWindow)
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

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
        return new IntPtr(1);
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

        GC.SuppressFinalize(this);
    }
    ~KeyboardHook()
    {
        Dispose();
    }
}