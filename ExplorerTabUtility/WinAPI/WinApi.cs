// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.WinAPI;

public static class WinApi
{
    public const int EVENT_OBJECT_CREATE = 0x8000;

    public const int VK_WIN = 0x5B; // Windows key code
    public const int VK_E = 0x45;   // E key code

    public const int WM_KEYDOWN = 0x0100; // Key down flag
    public const int WM_SETFOCUS = 0x0007; // Set Keyboard focus

    public const int SW_HIDE = 0; // Hide window
    public const int SW_SHOWNOACTIVATE = 4; // Show window but not activated
    public const int SWP_NOSIZE = 0x0001; // Retains the current size
    public const int SWP_NOZORDER = 0x0004; // Retains the current Z order

    public const int GW_ENABLEDPOPUP = 6; // Get the popup window owned by the specified window

    [DllImport("kernel32.dll")]
    public static extern nint LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool FreeLibrary(nint hModule);

    [DllImport("user32.dll")]
    public static extern nint SetWinEventHook(uint eventMin, uint eventMax, nint hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern bool UnhookWinEvent(nint hWinEventHook);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint SetWindowsHookEx(WinHookType HookType, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindowEx(nint parentHandle, nint childAfter, string className, string? windowTitle);

    [DllImport("user32.dll")]
    public static extern nint GetWindow(nint hWnd, uint uCmd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint handle, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(nint handle);

    [DllImport("user32.dll")]
    public static extern uint RealGetWindowClass(nint hwnd, StringBuilder pszType, uint cchType);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    public static nint GetAnotherExplorerWindow(nint currentWindow)
    {
        return currentWindow == default
            ? FindWindow("CabinetWClass", default)
            : FindAllWindowsEx()
                .FirstOrDefault(window => window != currentWindow);
    }
    public static nint ListenForNewExplorerTab(IReadOnlyCollection<nint> currentTabs, int searchTimeMs = 1000)
    {
        return Helper.DoUntilNotDefault(() =>
                GetAllExplorerTabs()
                    .Except(currentTabs)
                    .FirstOrDefault(),
            searchTimeMs);
    }
    public static List<nint> GetAllExplorerTabs()
    {
        var tabs = new List<nint>();

        foreach (var window in FindAllWindowsEx())
            tabs.AddRange(FindAllWindowsEx("ShellTabWindowClass", window));

        return tabs;
    }
    public static IEnumerable<nint> FindAllWindowsEx(string className = "CabinetWClass", nint parent = 0, string? windowTitle = default)
    {
        var handle = IntPtr.Zero;
        do
        {
            handle = FindWindowEx(parent, handle, className, windowTitle);

            if (handle == default) continue;

            yield return handle;

        } while (handle != default);
    }
    
    /// <summary>
    /// Hides the specified window by moving it outside the visible screen area.
    /// </summary>
    /// <param name="hWnd">The handle to the window that needs to be hidden.</param>
    /// <returns>The original position and size of the window before it was hidden, represented as a RECT structure.</returns>
    public static RECT HideWindow(nint hWnd)
    {
        GetWindowRect(hWnd, out var originalRect);

        // Move the window outside the screen (Hide)
        SetWindowPos(hWnd, IntPtr.Zero, -1000, -1000, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        return originalRect;
    }

    /// <summary>
    /// Restores the specified window to the foreground even if it was minimized.
    /// </summary>
    /// <param name="window">The handle to the window that needs to be restored to the foreground.</param>
    public static void RestoreWindowToForeground(nint window)
    {
        //If Minimized
        if (IsIconic(window))
        {
            // Show the window but don't activate it (otherwise won't respond to hot-keys), SetForegroundWindow gonna activate it. 
            ShowWindow(window, SW_SHOWNOACTIVATE);
        }

        //SetForeground
        SetForegroundWindow(window);
    }

    public static string GetWindowClassName(nint hWnd, int maxClassNameLength = 254)
    {
        if (hWnd == IntPtr.Zero) return string.Empty;

        var className = new StringBuilder(maxClassNameLength);
        _ = RealGetWindowClass(hWnd, className, (uint)(maxClassNameLength + 1));

        return className.ToString();
    }
    public static bool IsWindowHasClassName(nint hWnd, string className, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        var currentClassName = GetWindowClassName(hWnd, className.Length);

        return string.Equals(currentClassName, className, comparison);
    }
}