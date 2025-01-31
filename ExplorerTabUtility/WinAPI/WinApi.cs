// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.WinAPI;

public static class WinApi
{
    public const int EVENT_OBJECT_SHOW = 0x8002;

    public const int WM_SETREDRAW = 0xB; // Allow or prevent changes in a window from being redrawn
    public const int WM_COMMAND = 0x111; // Send a command

    public const int SW_SHOWNOACTIVATE = 4; // Show window but not activated

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

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint handle, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern bool SetLayeredWindowAttributes(nint hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetLayeredWindowAttributes(nint hwnd, out uint pcrKey, out byte pbAlpha, out uint pdwFlags);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(nint handle);

    [DllImport("user32.dll")]
    public static extern uint RealGetWindowClass(nint hwnd, StringBuilder pszType, uint cchType);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("shell32.dll", SetLastError = true)]
    public static extern int SHOpenFolderAndSelectItems(nint pidlFolder, uint cIdl, [In, MarshalAs(UnmanagedType.LPArray)] nint[] apidl, uint dwFlags);

    [DllImport("shell32.dll")]
    public static extern int SHGetDesktopFolder(out nint ppshf);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern void SHGetNameFromIDList(nint pidl, uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string? ppszName);

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
            // Show the window but don't activate it (otherwise won't respond to hot-keys), SetForegroundWindow going to activate it. 
            ShowWindow(window, SW_SHOWNOACTIVATE);
        }

        //SetForeground
        SetForegroundWindow(window);
    }

    private static void EnsureWindowIsLayered(nint hWnd)
    {
        var exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        if ((exStyle & WS_EX_LAYERED) != 0) return;

        SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
    }

    /// <summary>
    /// Sets the transparency of the specified window.
    /// </summary>
    /// <param name="hWnd">A handle to the window to set the transparency for.</param>
    /// <param name="alpha">The transparency value to set for the window.
    /// A value of 0 makes the window completely transparent, and a value of 255 makes the window opaque.</param>
    public static void SetWindowTransparency(nint hWnd, byte alpha = 128)
    {
        if (hWnd == 0) return;

        // Clamp the alpha value between 0 and 255
        alpha = alpha.Clamp(byte.MinValue, byte.MaxValue);

        // Ensure the window is layered
        EnsureWindowIsLayered(hWnd);

        // Set the transparency (alpha value) of the window (0 = transparent, 255 = opaque)
        SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
    }

    /// <summary>
    /// Retrieves the alpha transparency value of the specified window.
    /// </summary>
    /// <param name="hWnd">Handle to the window.</param>
    /// <returns>Alpha value (0-255) if successful; otherwise, null.</returns>
    public static byte? GetWindowTransparency(nint hWnd)
    {
        // Ensure the window is layered
        EnsureWindowIsLayered(hWnd);

        var success = GetLayeredWindowAttributes(hWnd, out _, out var pbAlpha, out var dwFlags);
        if (!success) return null;

        if ((dwFlags & LWA_ALPHA) != 0)
            return pbAlpha;

        // The window doesn't use alpha blending
        return null;
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

    public static void SuspendDrawing(this System.Windows.Forms.Control target)
    {
        SendMessage(target.Handle, WM_SETREDRAW, 0, 0);
    }
    public static void ResumeDrawing(this System.Windows.Forms.Control target, bool redraw = true)
    {
        SendMessage(target.Handle, WM_SETREDRAW, 1, 0);
        if (redraw) target.Refresh();
    }
}