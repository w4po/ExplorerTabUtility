// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using ExplorerTabUtility.Interop;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.WinAPI;

public static class WinApi
{
    public const int EVENT_OBJECT_SHOW = 0x8002;

    public const int WM_COMMAND = 0x111; // Send a command

    public const int SW_SHOWNOACTIVATE = 4; // Show window but not activated

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_FRAMECHANGED = 0x0020;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public const uint SWP_HIDEWINDOW = 0x0080;

    public const int GWL_EXSTYLE = -20; // Extended window style.
    public const int WS_EX_LAYERED = 0x80000; // Layered window.
    public const int LWA_ALPHA = 0x2; // Determine the opacity of a layered window

    public const uint SIGDN_URL = 0x80068000;

    [DllImport("user32.dll")]
    public static extern nint SetWinEventHook(uint eventMin, uint eventMax, nint hModWinEventProc, WinEventDelegate lPfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern bool UnhookWinEvent(nint hWinEventHook);

    [DllImport("user32.dll")]
    public static extern nint GetParent(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindowEx(nint parentHandle, nint childAfter, string className, string? windowTitle);

    [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "MapVirtualKeyW")]
    public static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint handle, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(nint handle);

    [DllImport("user32.dll")]
    public static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern bool SetLayeredWindowAttributes(nint hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern uint RealGetWindowClass(nint hwnd, StringBuilder pszType, uint cchType);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint hObject);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool QueryFullProcessImageName(nint hProcess, uint dwFlags, StringBuilder lpExeName, ref int lpdwSize);
    
    [DllImport("shell32.dll", SetLastError = true)]
    public static extern int SHOpenFolderAndSelectItems(nint pidlFolder, uint cIdl, [In, MarshalAs(UnmanagedType.LPArray)] nint[] apidl, uint dwFlags);

    [DllImport("shell32.dll")]
    public static extern int SHGetDesktopFolder(out nint ppshf);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern int SHGetNameFromIDList(nint pidl, uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string? ppszName);

    [DllImport("oleacc.dll")]
    public static extern nint AccessibleObjectFromPoint(Point pt, [Out, MarshalAs(UnmanagedType.Interface)] out IAccessible accObj, [Out] out object ChildID);

    public static IEnumerable<nint> FindAllWindowsEx(string className, nint parent = 0, string? windowTitle = null)
    {
        nint handle = 0;
        do
        {
            handle = FindWindowEx(parent, handle, className, windowTitle);

            if (handle == 0) continue;

            yield return handle;

        } while (handle != 0);
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
            // Show the window but don't activate it, SetForegroundWindow is going to activate it. 
            ShowWindow(window, SW_SHOWNOACTIVATE);
        }

        if (SetForegroundWindow(window)) return;

        Helper.BypassWinForegroundRestrictions();

        SetForegroundWindow(window);
    }

    public static string GetWindowClassName(nint hWnd, int maxClassNameLength = 254)
    {
        if (hWnd == 0) return string.Empty;

        var className = new StringBuilder(maxClassNameLength + 1);
        RealGetWindowClass(hWnd, className, (uint)className.Capacity);

        return className.ToString();
    }
    public static bool IsWindowHasClassName(nint hWnd, string className, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        var currentClassName = GetWindowClassName(hWnd, className.Length);

        return string.Equals(currentClassName, className, comparison);
    }
    
    public static string? GetProcessPath(int pid)
    {
        const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        var procHandle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, (uint)pid);
        if (procHandle == 0) return null;
        
        try
        {
            var capacity = 260;
            var sb = new StringBuilder(capacity);
            return QueryFullProcessImageName(procHandle, 0, sb, ref capacity) ? sb.ToString() : null;
        }
        finally
        {
            CloseHandle(procHandle);
        }
    }
}