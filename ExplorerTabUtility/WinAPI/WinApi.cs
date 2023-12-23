// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.WinAPI;

public static class WinApi
{
    public const int EVENT_OBJECT_CREATE = 0x8000;

    public const int WM_SETFOCUS = 0x0007; // Set Keyboard focus
    public const int WM_SETREDRAW = 0xB; // Allow or prevent changes in a window from being redrawn
    public const int WM_GETTEXT = 0x000D; // Get the text of a window

    public const int SW_HIDE = 0; // Hide window
    public const int SW_SHOWNOACTIVATE = 4; // Show window but not activated
    public const int SWP_NOSIZE = 0x0001; // Retains the current size
    public const int SWP_NOZORDER = 0x0004; // Retains the current Z order

    public const int GW_OWNER = 4; // Get the specified window's owner window, if any
    public const int GW_ENABLEDPOPUP = 6; // Get the enabled popup window owned by the specified window
    public const int GWL_STYLE = -16; // window style.
    public const int GWL_EXSTYLE = -20; // Extended window style.
    public const int WS_EX_LAYERED = 0x80000; // Layered window.
    public const uint WS_POPUP = 0x80000000; // popup.
    public const int LWA_ALPHA = 0x2; // Determine the opacity of a layered window

    [DllImport("kernel32.dll")]
    public static extern nint LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool FreeLibrary(nint hModule);

    [DllImport("user32.dll")]
    public static extern nint SetWinEventHook(uint eventMin, uint eventMax, nint hModWinEventProc, WinEventDelegate lPfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern bool UnhookWinEvent(nint hWinEventHook);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint SetWindowsHookEx(WinHookType HookType, HookProc lPfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindowEx(nint parentHandle, nint childAfter, string className, string? windowTitle);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint handle, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern nint GetWindow(nint hWnd, uint uCmd);

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

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(nint handle);

    [DllImport("user32.dll")]
    public static extern uint RealGetWindowClass(nint hwnd, StringBuilder pszType, uint cchType);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, StringBuilder lParam);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern nint ILCreateFromPathW(string pszPath);

    [DllImport("shell32.dll")]
    public static extern void ILFree(nint pidl);

    [DllImport("shell32.dll", SetLastError = true)]
    public static extern int SHOpenFolderAndSelectItems(nint pidlFolder, uint cIdl, [In, MarshalAs(UnmanagedType.LPArray)] nint[] apidl, uint dwFlags);

    public static bool IsFileExplorerForeground(out nint foregroundWindow)
    {
        foregroundWindow = GetForegroundWindow();
        return IsWindowHasClassName(foregroundWindow, "CabinetWClass");
    }
    public static nint GetAnotherExplorerWindow(nint currentWindow)
    {
        return currentWindow == default
            ? FindWindow("CabinetWClass", default)
            : FindAllWindowsEx("CabinetWClass")
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
    public static Task<nint> ListenForNewExplorerTabAsync(IReadOnlyCollection<nint> currentTabs, int searchTimeMs = 1000)
    {
        return Helper.DoUntilNotDefaultAsync(() =>
                GetAllExplorerTabs()
                    .Except(currentTabs)
                    .FirstOrDefault(),
            searchTimeMs);
    }
    public static List<nint> GetAllExplorerTabs()
    {
        var tabs = new List<nint>();

        foreach (var window in FindAllWindowsEx("CabinetWClass"))
            tabs.AddRange(FindAllWindowsEx("ShellTabWindowClass", window));

        return tabs;
    }
    public static IEnumerable<nint> FindAllWindowsEx(string className, nint parent = 0, string? windowTitle = default)
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
    /// Finds a popup window associated with a specified owner window.
    /// </summary>
    /// <param name="owner">The handle of the owner window.</param>
    /// <param name="popupCaption">Optional. The caption of the popup window. If not specified, the method will return the first popup window found.</param>
    /// <param name="popupClassName">Optional. The class name of the popup window. If not specified, the method will return the first popup window found.</param>
    /// <returns>The handle of the popup window if found, otherwise returns default value.</returns>
    public static nint FindPopupWindow(nint owner, string? popupCaption = default, string? popupClassName = default)
    {
        nint popupHWnd = default;
        EnumWindows((hWnd, _) =>
        {
            var style = GetWindowLong(hWnd, GWL_STYLE);
            var isPopup = (style & WS_POPUP) == WS_POPUP;
            if (!isPopup) return true;

            var windowOwner = GetWindow(hWnd, GW_OWNER);
            if (windowOwner != owner) return true;

            // If the caption is specified, check if it matches
            if (popupCaption?.Length > 0)
            {
                var caption = GetWindowCaption(hWnd, popupCaption.Length);
                
                // If the caption doesn't match, continue enumerating windows
                if (!string.Equals(caption, popupCaption, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // If the class name is specified, check if it matches
            if (popupClassName?.Length > 0)
            {
                var windowClassName = GetWindowClassName(hWnd, popupClassName.Length);
                
                // If the class name doesn't match, continue enumerating windows
                if (!string.Equals(windowClassName, popupClassName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            popupHWnd = hWnd;
            return false; // Stop enumerating windows
        }, default);

        return popupHWnd;
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
            // Show the window but don't activate it (otherwise won't respond to hot-keys), SetForegroundWindow going to activate it. 
            ShowWindow(window, SW_SHOWNOACTIVATE);
        }

        //SetForeground
        SetForegroundWindow(window);
    }

    /// <summary>
    /// Sets the transparency of the specified window.
    /// </summary>
    /// <param name="hWnd">A handle to the window to set the transparency for.</param>
    /// <param name="alpha">The transparency value to set for the window.
    /// A value of 0 makes the window completely transparent, and a value of 255 makes the window opaque.</param>
    public static void SetWindowTransparency(nint hWnd, byte alpha = 128)
    {
        if (hWnd == default) return;

        // Clamp the alpha value between 0 and 255
        alpha = alpha.Clamp(byte.MinValue, byte.MaxValue);

        // Get the current extended window style
        var extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

        // Make the window layered
        SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);

        // Set the transparency (alpha value) of the window (0 = transparent, 255 = opaque)
        SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
    }

    public static string GetWindowCaption(nint hWnd, int maxCaptionLength = 254)
    {
        if (hWnd == default) return string.Empty;

        var caption = new StringBuilder(maxCaptionLength + 1);
        SendMessage(hWnd, WM_GETTEXT, caption.Capacity, caption);

        return caption.ToString();
    }
    public static string GetWindowClassName(nint hWnd, int maxClassNameLength = 254)
    {
        if (hWnd == default) return string.Empty;

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