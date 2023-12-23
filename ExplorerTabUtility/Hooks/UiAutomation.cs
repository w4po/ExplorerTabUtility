using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FlaUI.UIA3;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.Core.AutomationElements;
using ExplorerTabUtility.WinAPI;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.Helpers;
using Window = ExplorerTabUtility.Models.Window;

namespace ExplorerTabUtility.Hooks;

public class UiAutomation(Func<Window, Task> onNewWindow) : IHook
{
    private readonly HWndCache _cache = new(5_000);
    private static readonly UIA3Automation Automation = new();
    public bool IsHookActive { get; private set; }

    public void StartHook()
    {
        Automation
            .GetDesktop()
            .RegisterAutomationEvent(Automation.EventLibrary.Window.WindowOpenedEvent, TreeScope.Children, OnWindowOpenHandler);
        IsHookActive = true;
    }
    public void StopHook()
    {
        Automation.UnregisterAllEvents();
        IsHookActive = false;
    }

    private async void OnWindowOpenHandler(AutomationElement element, EventId _)
    {
        if (element.ClassName != "CabinetWClass") return;
        if (WinApi.FindAllWindowsEx("CabinetWClass").Take(2).Count() < 2) return;

        var hWnd = element.Properties.NativeWindowHandle.Value;

        // The event gets triggered twice for some reason, we have to catch it only once.
        if (_cache.Contains(hWnd)) return;
        _cache.Add(hWnd);

        var originalRect = WinApi.HideWindow(hWnd);
        var showAgain = true;
        try
        {
            // A new "This PC" window (unless the opened folder is called "This PC"?)
            if (string.Equals(element.Name, "This PC"))
            {
                await CloseAndNotifyNewWindowAsync(element, new Window(string.Empty, oldWindowHandle: hWnd)).ConfigureAwait(false);
                showAgain = false;
                return;
            }

            var header = await GetHeaderElementsAsync(element).ConfigureAwait(false);
            if (header?.SuggestBox == null || header.AddressBar == default) return;

            // We have to invoke the suggestBox to populate the address bar
            header.SuggestBox.Patterns.Invoke.Pattern.Invoke();

            var location = await Helper.DoUntilConditionAsync(
                action: () => header.AddressBar.Patterns.Value.Pattern.Value.Value,
                predicate: l => !string.IsNullOrWhiteSpace(l))
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(location))
                return;

            // ("Home") For English version.
            if (string.Equals(location, "Home"))
            {
                await CloseAndNotifyNewWindowAsync(element, new Window(string.Empty, oldWindowHandle: hWnd)).ConfigureAwait(false);
                showAgain = false;
                return;
            }

            var tab = element.FindFirstChild(c => c.ByClassName("ShellTabWindowClass"));

            var folderView = tab?.FindFirstDescendant(c => c.ByClassName("UIItemsView"));
            if (folderView == default)
            {
                // ("Home") For non-English versions.
                var home = tab?.FindFirstDescendant(c => c.ByClassName("HomeListView"));
                if (home == default) return;

                await CloseAndNotifyNewWindowAsync(element, new Window(string.Empty, oldWindowHandle: hWnd)).ConfigureAwait(false);
                showAgain = false;
                return;
            }

            var selectedNames = folderView.Patterns.Selection.Pattern.Selection.Value
                ?.Select(s => s.Name)
                .ToList();

            var tabHWnd = tab!.Properties.NativeWindowHandle.Value;

            await CloseAndNotifyNewWindowAsync(element, new Window(location, selectedNames, hWnd, tabHWnd)).ConfigureAwait(false);
            showAgain = false;
        }
        finally
        {
            // Move the back to the screen (Show)
            if (showAgain)
                WinApi.SetWindowPos(hWnd, default, originalRect.Left, originalRect.Top, 0, 0, WinApi.SWP_NOSIZE | WinApi.SWP_NOZORDER);
        }
    }

    public static AutomationElement? FromHandle(nint hWnd) => Automation.FromHandle(hWnd);
    public static async Task<string?> GetCurrentTabLocationAsync(AutomationElement window)
    {
        var header = await GetHeaderElementsAsync(window).ConfigureAwait(false);
        if (header?.SuggestBox == default || header.AddressBar == default) return default;

        // Give the address bar focus to update its value with the current location.
        header.AddressBar.Focus();

        var windowHandle = window.Properties.NativeWindowHandle.Value;
        var tabHandle = window
            .FindFirstChild(c => c.ByClassName("ShellTabWindowClass"))
            .Properties.NativeWindowHandle.Value;

        if (tabHandle == default)
            tabHandle = windowHandle;

        // Wait in the background for 700ms and give the focus to the tab to close the address bar.
        _ = Helper.DoDelayedBackgroundAsync(() => WinApi.PostMessage(tabHandle, WinApi.WM_SETFOCUS, 0, 0), 700);

        return await Helper.DoUntilConditionAsync<string?>(
                action: () => header.AddressBar.Patterns.Value.Pattern.Value.Value,
                predicate: l => !string.IsNullOrWhiteSpace(l))
            .ConfigureAwait(false);
    }
    public static bool AddNewTab(AutomationElement window)
    {
        var addButton = GetAddNewTabButton(window);
        if (addButton == default) return false;

        addButton.Patterns.Invoke.Pattern.Invoke();
        return true;
    }
    public static AutomationElement? GetAddNewTabButton(AutomationElement window)
    {
        var headerBar = window.FindFirstChild(c => c.ByClassName("Microsoft.UI.Content.DesktopChildSiteBridge"));
        var tabView = headerBar?.FindFirstChild("TabView");
        var addButton = tabView?.FindFirstChild("AddButton");

        return addButton;
    }
    public static async Task<bool> NavigateAsync(AutomationElement window, nint tabHandle, string location)
    {
        var header = await GetHeaderElementsAsync(window).ConfigureAwait(false);
        if (header?.SuggestBox == default || header.AddressBar == default)
            return false;

        // Set the location.
        header.AddressBar.Patterns.Value.Pattern.SetValue(location);

        // We have to invoke the suggestBox to Navigate :(
        header.SuggestBox.Patterns.Invoke.Pattern.Invoke();

        // Wait in the background for 700ms and give the focus to the tab to close the address bar.
        _ = Helper.DoDelayedBackgroundAsync(() => WinApi.PostMessage(tabHandle, WinApi.WM_SETFOCUS, 0, 0), 700);

        return true;
    }
    public static async Task<bool> SelectItemsAsync(AutomationElement tab, ICollection<string> names)
    {
        if (names.Count == 0) return false;

        var condition = new PropertyCondition(Automation.PropertyLibrary.Element.ClassName, "UIItemsView");
        var itemsView = await Helper.DoUntilNotDefaultAsync(
            () => tab.FindFirstWithOptions(TreeScope.Subtree, condition, TreeTraversalOptions.Default, tab))
            .ConfigureAwait(false);

        var files = itemsView?.FindAllChildren();
        if (files == default) return false;

        var selectedCount = 0;
        foreach (var fileElement in files)
        {
            if (!names.Any(n => n.Equals(fileElement.Name, StringComparison.OrdinalIgnoreCase)))
                continue;

            fileElement.Patterns.SelectionItem.Pattern.AddToSelection();

            if (++selectedCount >= names.Count) return true;
        }

        // At least one is selected.
        return selectedCount > 0;
    }
    public static async Task<bool> CloseRandomTabOfAWindowAsync(AutomationElement window)
    {
        var headerBar = await Helper.DoUntilNotDefaultAsync(
            () => window.FindFirstChild(c => c.ByClassName("Microsoft.UI.Content.DesktopChildSiteBridge")))
            .ConfigureAwait(false);

        if (headerBar == default) return false;

        var closeButton = await Helper.DoUntilNotDefaultAsync(() => headerBar.FindFirstDescendant("CloseButton")).ConfigureAwait(false);

        var invokePattern = closeButton?.Patterns.Invoke.Pattern;
        if (invokePattern == default)
            return false;

        invokePattern.Invoke();

        return true;
    }
    public static async Task<WindowHeaderElements?> GetHeaderElementsAsync(AutomationElement window)
    {
        var headerBar = await Helper.DoUntilNotDefaultAsync(
            action: () => window.FindFirstChild(c => c.ByClassName("Microsoft.UI.Content.DesktopChildSiteBridge")))
            .ConfigureAwait(false);

        if (headerBar == default) return default;

        var suggestBox = headerBar.FindFirstChild("PART_AutoSuggestBox");
        var addressBar = suggestBox?.FindFirstChild(c => c.ByName("Address Bar"));

        return new WindowHeaderElements(suggestBox, addressBar);
    }
    private async Task CloseAndNotifyNewWindowAsync(AutomationElement element, Window window)
    {
        // the window suppose to have only one tab, so we should be okay.
        await CloseRandomTabOfAWindowAsync(element).ConfigureAwait(false);

        _ = onNewWindow.Invoke(window);
    }

    public void Dispose()
    {
        StopHook();
        Automation.Dispose();
        GC.SuppressFinalize(this);
    }
    ~UiAutomation()
    {
        Dispose();
    }
}