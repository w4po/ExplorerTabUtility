using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FlaUI.Core.AutomationElements;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;
using Keyboard = ExplorerTabUtility.Hooks.Keyboard;
using Window = ExplorerTabUtility.Models.Window;

namespace ExplorerTabUtility.Managers;

public static class InteractionManager
{
    public static InteractionMethod InteractionMethod { get; set; }
    private static readonly SemaphoreSlim Limiter = new(1);
    private static nint _mainWindowHandle;

    public static async void OnHotKeyProfileTriggered(HotKeyProfile profile)
    {
        // If the interaction method is via Keyboard,
        // Allow a brief delay for the user to release the keys,
        // as not doing so might result in unexpected behavior.
        if (InteractionMethod == InteractionMethod.Keyboard)
            await Task.Delay(250).ConfigureAwait(false);
        
        switch (profile.Action)
        {
            case HotKeyAction.Open: await ShortcutOpenNewTab(profile.Path, profile.Delay).ConfigureAwait(false); break;
            case HotKeyAction.Duplicate: await ShortcutDuplicateCurrentTab(profile.Delay).ConfigureAwait(false); break;
            default: throw new ArgumentOutOfRangeException(nameof(profile), profile.Action, @"Invalid profile action");
        }
    }

    public static async void OnNewWindow(Window window, bool openNewIfNonExist = false)
    {
        var windowHandle = IntPtr.Zero;
        try
        {
            await Limiter.WaitAsync().ConfigureAwait(false);

            windowHandle = GetMainWindowHWnd(window.OldWindowHandle);
            if (windowHandle == default && !openNewIfNonExist) return;
            if (windowHandle == default)
            {
                Shell32.OpenNewWindowAndSelectItems(window.Path, window.SelectedItems);
                return;
            }

            var windowElement = UiAutomation.FromHandle(windowHandle);
            if (windowElement == default) return;

            // Hide the popup (suggestion window).
            var popupWindow = WinApi.FindPopupWindow(windowHandle, "PopupHost");
            WinApi.SetWindowTransparency(popupWindow, 0);

            // Store currently opened Tabs, before we open a new one.
            var oldTabs = WinApi.GetAllExplorerTabs();

            // Add new tab.
            await AddNewTabAsync(windowElement).ConfigureAwait(false);

            // If it is just a new (This PC | Home), return.
            if (string.IsNullOrWhiteSpace(window.Path)) return;

            // Get newly created tab's handle (That is not in 'oldTabs')
            var newTabHandle = await WinApi.ListenForNewExplorerTabAsync(oldTabs).ConfigureAwait(false);
            if (newTabHandle == default) return;

            // Navigate to the target location
            await NavigateAsync(windowElement, newTabHandle, window.Path).ConfigureAwait(false);

            if (window.SelectedItems is not { Count: > 0 } selectedItems) return;

            // Select items
            await SelectItemsAsync(newTabHandle, selectedItems).ConfigureAwait(false);
        }
        finally
        {
            if (windowHandle != default)
                WinApi.RestoreWindowToForeground(windowHandle);

            Limiter.Release();
        }
    }
    private static nint GetMainWindowHWnd(nint otherThan)
    {
        if (WinApi.IsWindowHasClassName(_mainWindowHandle, "CabinetWClass"))
            return _mainWindowHandle;

        var allWindows = WinApi.FindAllWindowsEx("CabinetWClass");

        // Get another handle other than the newly created one. (In case if it is still alive.)
        _mainWindowHandle = allWindows.FirstOrDefault(t => t != otherThan);

        return _mainWindowHandle;
    }
    private static Task AddNewTabAsync(AutomationElement window)
    {
        // if via UI is selected try to add a new tab with UI Automation.
        if (InteractionMethod == InteractionMethod.UiAutomation && UiAutomation.AddNewTab(window))
            return Task.CompletedTask;

        // Via Keys is selected or UI Automation fails.
        return Keyboard.AddNewTabAsync(window.Properties.NativeWindowHandle.Value);
    }
    private static async Task NavigateAsync(AutomationElement window, nint tabHandle, string location)
    {
        // if via UI is selected try to Navigate with UI Automation.
        if (InteractionMethod == InteractionMethod.UiAutomation &&
            await UiAutomation.NavigateAsync(window, tabHandle, location).ConfigureAwait(false))
            return;

        // Via Keys is selected or UI Automation fails.
        var windowHandle = window.Properties.NativeWindowHandle.Value;
        await Keyboard.NavigateAsync(windowHandle, location).ConfigureAwait(false);
    }
    private static async Task SelectItemsAsync(nint tabHandle, ICollection<string> names)
    {
        // if via UI is selected try to Select with UI Automation.
        if (InteractionMethod == InteractionMethod.UiAutomation)
        {
            var tabElement = UiAutomation.FromHandle(tabHandle);
            if (tabElement != default && await UiAutomation.SelectItemsAsync(tabElement, names).ConfigureAwait(false))
                return;
        }

        // Via Keys is selected or UI Automation fails.
        await Keyboard.SelectItemsAsync(tabHandle, names).ConfigureAwait(false);
    }

    private static async Task<string?> GetCurrentForegroundTabLocation()
    {
        if (!WinApi.IsFileExplorerForeground(out var foregroundWindow))
            return default;

        // Hide the popup (suggestion window).
        var popupWindow = WinApi.FindPopupWindow(foregroundWindow, "PopupHost");
        WinApi.SetWindowTransparency(popupWindow, 0);

        string? currentTabLocation;
        if (InteractionMethod == InteractionMethod.UiAutomation)
        {
            var windowElement = UiAutomation.FromHandle(foregroundWindow);
            if (windowElement == default) return default;

            currentTabLocation = await UiAutomation.GetCurrentTabLocationAsync(windowElement).ConfigureAwait(false);
            if (currentTabLocation != default) return currentTabLocation;
        }

        currentTabLocation = await Keyboard.GetCurrentTabLocationAsync(foregroundWindow, false).ConfigureAwait(false);

        return currentTabLocation;
    }
    private static async Task ShortcutOpenNewTab(string? path, int delay)
    {
        if (delay > 0)
            await Task.Delay(delay).ConfigureAwait(false);

        OnNewWindow(new Window(path ?? string.Empty), true);
    }
    private static async Task ShortcutDuplicateCurrentTab(int delay)
    {
        string? currentTabLocation;
        try
        {
            await Limiter.WaitAsync().ConfigureAwait(false);
            currentTabLocation = await GetCurrentForegroundTabLocation().ConfigureAwait(false);
            if (currentTabLocation == default) return;
        }
        finally
        {
            Limiter.Release();
        }

        if (delay > 0)
            await Task.Delay(delay).ConfigureAwait(false);

        OnNewWindow(new Window(currentTabLocation), true);
    }
}