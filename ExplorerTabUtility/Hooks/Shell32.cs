using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Hooks;

public class Shell32(Action<Window, bool> onNewWindow) : IHook
{
    private nint _hookId;
    private WinEventDelegate? _eventHookCallback; // We have to keep a reference because of GC
    private static object? _shell;
    private static Type? _shellType;
    private static Type? _windowType;
    public bool IsHookActive { get; private set; }

    public void StartHook()
    {
        _eventHookCallback = OnWindowOpenHandler;
        _hookId = WinApi.SetWinEventHook(WinApi.EVENT_OBJECT_CREATE, WinApi.EVENT_OBJECT_CREATE, default, _eventHookCallback, 0, 0, 0);
        IsHookActive = true;
    }
    public void StopHook()
    {
        WinApi.UnhookWinEvent(_hookId);
        IsHookActive = false;
    }

    private void OnWindowOpenHandler(nint hWinEventHook, uint eventType, nint hWnd, int idObject, int idChild, uint dwEventThread, uint dWmsEventTime)
    {
        if (!WinApi.IsWindowHasClassName(hWnd, "CabinetWClass")) return;
        if (WinApi.FindAllWindowsEx("CabinetWClass").Take(2).Count() < 2) return;

        var originalRect = WinApi.HideWindow(hWnd);
        var showAgain = true;
        try
        {
            var window = GetWindowByHandle(GetWindows(), hWnd);
            if (window == default) return;

            var location = GetWindowLocation(window);

            // Home
            if (location == string.Empty)
            {
                CloseAndNotifyNewWindow(window, new Window(string.Empty, oldWindowHandle: hWnd));
                showAgain = false;
                return;
            }

            if (!Uri.TryCreate(location, UriKind.Absolute, out var uri))
                return;

            // Give the selection some time to take effect.
            Thread.Sleep(70);
            var selectedItems = GetSelectedItems(window);

            CloseAndNotifyNewWindow(window, new Window(uri.LocalPath, selectedItems, oldWindowHandle: hWnd));
            showAgain = false;
        }
        finally
        {
            // Move the back to the screen (Show)
            if (showAgain)
                WinApi.SetWindowPos(hWnd, default, originalRect.Left, originalRect.Top, 0, 0, WinApi.SWP_NOSIZE | WinApi.SWP_NOZORDER);
        }
    }

    public static object? GetWindows()
    {
        _shellType ??= Type.GetTypeFromProgID("Shell.Application");
        if (_shellType == default) return default;

        _shell ??= Activator.CreateInstance(_shellType);
        if (_shell == default) return default;

        var windows = _shellType.InvokeMember("Windows", BindingFlags.InvokeMethod, null, _shell, []);
        _windowType ??= windows?.GetType();

        return windows;
    }

    /// <summary>
    /// Retrieves the current location of the specified window.
    /// <para>Applies only to the first tab of a window.</para>
    /// </summary>
    /// <param name="window">The window object for which to get the location. This should be an instance of a window obtained from the Shell32 API.</param>
    /// <returns>The current location of the window as a string, or null if the window or window type is not defined.</returns>
    public static string? GetWindowLocation(object? window)
    {
        if (window == default || _windowType == default) return default;

        return _windowType.InvokeMember("LocationURL", BindingFlags.GetProperty, null, window, null) as string;
    }

    /// <summary>
    /// Navigates the specified window to a given location.
    /// <para>Applies only to the first tab of a window.</para>
    /// </summary>
    /// <param name="window">The window object to navigate. This should be an instance of a window obtained from the Shell32 API.</param>
    /// <param name="location">The location to navigate to.</param>
    public static void NavigateToLocation(object? window, string location)
    {
        if (window == default || _windowType == default) return;

        _windowType.InvokeMember("Navigate", BindingFlags.InvokeMethod, null, window, [location]);
    }

    private static int GetCount(object? item)
    {
        if (item == default || _windowType == default) return default;

        var obj = _windowType.InvokeMember("Count", BindingFlags.GetProperty, null, item, null);
        return obj is int count ? count : default;
    }
    public static object? GetWindowByHandle(object? windows, nint hWnd)
    {
        if (hWnd == default || windows == default || _windowType == default) return default;

        var count = GetCount(windows);
        for (var i = 0; i < count; i++)
        {
            var window = _windowType.InvokeMember("Item", BindingFlags.InvokeMethod, null, windows, [i]);
            if (window == default) continue;

            var itemHWnd = _windowType.InvokeMember("HWND", BindingFlags.GetProperty, null, window, null);
            if (itemHWnd == default) continue;

            if ((long)itemHWnd == hWnd)
                return window;
        }

        return default;
    }

    /// <summary>
    /// Retrieves the list of selected items in the specified window.
    /// <para>Applies only to the first tab of a window.</para>
    /// </summary>
    /// <param name="window">The window object for which to get the selected items. This should be an instance of a window obtained from the Shell32 API.</param>
    /// <returns>A list of selected items as strings, or null if the window, window type, or selected items are not defined.</returns>
    public static List<string>? GetSelectedItems(object? window)
    {
        if (window == default || _windowType == default) return default;

        var document = _windowType.InvokeMember("Document", BindingFlags.GetProperty, null, window, null);
        if (document == default) return default;

        var selectedItems = _windowType.InvokeMember("SelectedItems", BindingFlags.InvokeMethod, null, document, null);
        if (selectedItems == default) return default;

        var count = GetCount(selectedItems);
        if (count == 0) return default;

        var selectedList = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var selectedItem = _windowType.InvokeMember("Item", BindingFlags.InvokeMethod, null, selectedItems, [i]);
            if (selectedItem == default) return default;

            if (_windowType.InvokeMember("Name", BindingFlags.GetProperty, null, selectedItem, null) is string selectedItemName)
                selectedList.Add(selectedItemName);
        }

        return selectedList;
    }

    /// <summary>
    /// Selects the specified items in the given window.
    /// <para>Applies only to the first tab of a window.</para>
    /// </summary>
    /// <param name="window">The window object in which to select items. This should be an instance of a window obtained from the Shell32 API.</param>
    /// <param name="names">The collection of names of the items to be selected.</param>
    public static void SelectItems(object? window, ICollection<string>? names)
    {
        if (window == default || _windowType == default || names == default || names.Count == 0)
            return;

        var document = _windowType.InvokeMember("Document", BindingFlags.GetProperty, null, window, null);
        if (document == default) return;

        var folder = _windowType.InvokeMember("Folder", BindingFlags.GetProperty, null, document, null);
        if (folder == default) return;

        var files = _windowType.InvokeMember("Items", BindingFlags.InvokeMethod, null, folder, null);
        if (files == default) return;

        var count = GetCount(files);
        if (count == default) return;

        var selectedCount = 0;
        for (var i = 0; i < count; i++)
        {
            var item = _windowType.InvokeMember("Item", BindingFlags.InvokeMethod, null, files, [i]);
            if (item == default) return;

            var name = _windowType.InvokeMember("Name", BindingFlags.GetProperty, null, item, null) as string;

            if (!names.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)))
                continue;

            _windowType.InvokeMember("SelectItem", BindingFlags.InvokeMethod, null, document, [item, 1]);

            if (++selectedCount >= names.Count) return;
        }
    }

    public static void CloseWindow(object? window)
    {
        if (window == default || _windowType == default) return;

        _windowType.InvokeMember("Quit", BindingFlags.InvokeMethod, null, window, null);
    }
    private void CloseAndNotifyNewWindow(object? item, Window window)
    {
        if (item == default || _windowType == default) return;

        CloseWindow(item);

        Task.Run(() => onNewWindow.Invoke(window, false));
    }

    public static void OpenNewWindowAndSelectItems(string folderPath, ICollection<string>? names)
    {
        // If there are no names, add an empty string to the list, Otherwise the parent folder will open instead.
        if (names == default || names.Count == 0)
            names = new[] { string.Empty };
        
        var folderPidl = WinApi.ILCreateFromPathW(folderPath);
        if (folderPidl == default) return;

        var filesApidl = GetFilesPidls(folderPath, names);
        WinApi.SHOpenFolderAndSelectItems(folderPidl, (uint)filesApidl.Length, filesApidl, 0);

        WinApi.ILFree(folderPidl);
        if (filesApidl.Length == 0) return;

        foreach (var pidl in filesApidl) WinApi.ILFree(pidl);
    }
    private static nint[] GetFilesPidls(string folderPath, IEnumerable<string> fileNames)
    {
        return fileNames
            .Select(file => WinApi.ILCreateFromPathW(Path.Combine(folderPath, file)))
            .Where(nativeFile => nativeFile != default)
            .ToArray();
    }

    public void Dispose() => StopHook();
    ~Shell32() => Dispose();
}