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
    public const nint WM_KEYDOWN = 0x0100; // Key down flag

    public const int KEYEVENTF_KEYUP = 0x0002; // EventKey up flag
    public const int SW_SHOWNOACTIVATE = 4; // Show window but not activated

    public const int VK_WIN = 0x5B; // Windows key code
    public const int VK_CONTROL = 0x11; // CTRL key code
    public const int VK_E = 0x45;   // E key code
    public const int VK_T = 0x54;   // T key code

    public static uint SWP_NOSIZE = 0x0001;
    public static uint SWP_NOZORDER = 0x0004;


    [DllImport("kernel32.dll")]
    public static extern nint LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool FreeLibrary(nint hModule);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint SetWindowsHookEx(WinHookType HookType, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string? windowTitle);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint handle, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(nint handle);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(nint hWnd, out nint processId);

    [DllImport("user32.dll")]
    public static extern uint RealGetWindowClass(nint hwnd, StringBuilder pszType, uint cchType);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

    public static IntPtr GetAnotherExplorerWindow(IntPtr currentWindow)
    {
        return currentWindow == default
            ? FindWindow("CabinetWClass", default)
            : FindAllWindowsEx()
                .FirstOrDefault(window => window != currentWindow);
    }
    public static IntPtr ListenForNewExplorerTab(IReadOnlyCollection<IntPtr> currentTabs, int searchTimeMs = 1000)
    {
        return Helper.DoUntilNotDefault(() => 
                GetAllExplorerTabs()
                    .Except(currentTabs)
                    .FirstOrDefault(),
            searchTimeMs);
    }
    public static List<IntPtr> GetAllExplorerTabs()
    {
        var tabs = new List<IntPtr>();

        foreach (var window in FindAllWindowsEx())
            tabs.AddRange(FindAllWindowsEx("ShellTabWindowClass", window));

        return tabs;
    }
    public static IEnumerable<IntPtr> FindAllWindowsEx(string className = "CabinetWClass", nint parent = 0, string? windowTitle = default)
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
    /// Restores the specified window to the foreground even if it was minimized.
    /// </summary>
    /// <param name="window">The handle to the window that needs to be restored to the foreground.</param>
    public static void RestoreWindowToForeground(IntPtr window)
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

    public static bool IsWindowStillHasClassName(IntPtr hWnd, string className)
    {
        if (hWnd == IntPtr.Zero) return false;

        var currentClassName = new StringBuilder(className.Length + 1);
        _ = RealGetWindowClass(hWnd, currentClassName, (uint)(className.Length + 1));

        return string.Equals(currentClassName.ToString(), className, StringComparison.OrdinalIgnoreCase);
    }
}