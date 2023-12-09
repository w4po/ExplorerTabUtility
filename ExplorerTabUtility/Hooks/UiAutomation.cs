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

public class UiAutomation : IDisposable
{
    private readonly Func<Window, Task> _onNewWindow;
    private readonly HWndCache _cache = new(5_000);
    private static readonly UIA3Automation Automation = new();

    public UiAutomation(Func<Window, Task> onNewWindow)
    {
        _onNewWindow = onNewWindow;
    }

    public void StartHook()
    {
        Automation
            .GetDesktop()
            .RegisterAutomationEvent(Automation.EventLibrary.Window.WindowOpenedEvent, TreeScope.Children, OnWindowOpenHandler);
    }
    public void StopHook()
    {
        Automation.UnregisterAllEvents();
    }

    private void OnWindowOpenHandler(AutomationElement element, EventId _)
    {
        if (element.ClassName != "CabinetWClass") return;
        if (WinApi.FindAllWindowsEx().Take(2).Count() < 2) return;

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
                CloseAndNotifyNewWindow(element, new Window(string.Empty, oldWindowHandle: hWnd));
                showAgain = false;
                return;
            }

            GetHeaderElements(element, out var suggestBox, out var searchBox, out var addressBar);
            if (suggestBox == default || searchBox == default || addressBar == default) return;

            // We have to invoke the suggestBox to populate the address bar :(
            suggestBox.Patterns.Invoke.Pattern.Invoke();

            // Invoke searchBox to hide the suggestPopup window.
            searchBox.Patterns.Invoke.Pattern.Invoke();

            var location = Helper.DoUntilCondition(
                action: () => addressBar.Patterns.Value.Pattern.Value.Value,
                predicate: l => !string.IsNullOrWhiteSpace(l));

            if (string.IsNullOrWhiteSpace(location))
                return;

            // ("Home") For English version.
            if (string.Equals(location, "Home"))
            {
                CloseAndNotifyNewWindow(element, new Window(string.Empty, oldWindowHandle: hWnd));
                showAgain = false;
                return;
            }

            var tab = element.FindFirstChild(c => c.ByClassName("ShellTabWindowClass"));

            var folderView = tab?.FindFirstDescendant(c => c.ByClassName("UIItemsView"));
            if (folderView == default)
            {
                // ("Home") For non English versions.
                var home = tab?.FindFirstDescendant(c => c.ByClassName("HomeListView"));
                if (home == default) return;

                CloseAndNotifyNewWindow(element, new Window(string.Empty, oldWindowHandle: hWnd));
                showAgain = false;
                return;
            }

            var selectedNames = folderView.Patterns.Selection.Pattern.Selection.Value
                ?.Select(s => s.Name)
                .ToList();

            var tabHWnd = tab!.Properties.NativeWindowHandle.Value;

            CloseAndNotifyNewWindow(element, new Window(location, selectedNames, hWnd, tabHWnd));
            showAgain = false;
        }
        finally
        {
            // Move the back to the screen (Show)
            if (showAgain)
                WinApi.SetWindowPos(hWnd, IntPtr.Zero, originalRect.Left, originalRect.Top, 0, 0, WinApi.SWP_NOSIZE | WinApi.SWP_NOZORDER);
        }
    }
    public static AutomationElement? FromHandle(IntPtr hWnd) => Automation.FromHandle(hWnd);
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
    public static bool Navigate(AutomationElement window, nint tabHandle, string location)
    {
        GetHeaderElements(window, out var suggestBox, out var searchBox, out var addressBar);
        if (suggestBox == default || searchBox == default || addressBar == default)
            return false;

        // Set the location.
        addressBar.Patterns.Value.Pattern.SetValue(location);

        // We have to invoke the suggestBox to Navigate :(
        suggestBox.Patterns.Invoke.Pattern.Invoke();

        // Wait in the background
        Task.Run(async () =>
        {
            await Task.Delay(700).ConfigureAwait(false);

            var popupHandle = WinApi.GetWindow(window.Properties.NativeWindowHandle.Value, WinApi.GW_ENABLEDPOPUP);

            // If for some reason the address bar doesn't have the focus anymore, return.
            if (popupHandle == 0) return;

            // Hide Suggestion popup.
            WinApi.ShowWindow(popupHandle, WinApi.SW_HIDE);

            // Give the focus to the tab to close the address bar.
            WinApi.PostMessage(tabHandle, WinApi.WM_SETFOCUS, 0, 0);
        });

        return true;
    }
    public static bool SelectItems(AutomationElement tab, ICollection<string> names)
    {
        if (names.Count == 0) return false;

        var condition = new PropertyCondition(Automation.PropertyLibrary.Element.ClassName, "UIItemsView");
        var itemsView = Helper.DoUntilNotDefault(() => tab.FindFirstWithOptions(TreeScope.Subtree, condition, TreeTraversalOptions.Default, tab));

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
    public static bool CloseRandomTabOfAWindow(AutomationElement window)
    {
        var headerBar = Helper.DoUntilNotDefault(() => window.FindFirstChild(c => c.ByClassName("Microsoft.UI.Content.DesktopChildSiteBridge")));
        if (headerBar == default) return false;

        var closeButton = Helper.DoUntilNotDefault(() => headerBar.FindFirstDescendant("CloseButton"));

        var invokePattern = closeButton?.Patterns.Invoke.Pattern;
        if (invokePattern == default)
            return false;

        invokePattern.Invoke();

        return true;
    }
    private static void GetHeaderElements(AutomationElement window, out AutomationElement? suggestBox, out AutomationElement? searchBox, out AutomationElement? addressBar)
    {
        suggestBox = default;
        searchBox = default;
        addressBar = default;

        var headerBar = Helper.DoUntilNotDefault(() => window.FindFirstChild(c => c.ByClassName("Microsoft.UI.Content.DesktopChildSiteBridge")));
        if (headerBar == default) return;

        searchBox = Helper.DoUntilNotDefault(() => headerBar.FindFirstChild("FileExplorerSearchBox"));
        if (searchBox == default) return;

        suggestBox = headerBar.FindFirstChild("PART_AutoSuggestBox");
        addressBar = suggestBox?.FindFirstChild(c => c.ByName("Address Bar"));
    }
    private void CloseAndNotifyNewWindow(AutomationElement element, Window window)
    {
        // the window suppose to have only one tab, so we should be okay.
        CloseRandomTabOfAWindow(element);

        _onNewWindow.Invoke(window);
    }

    public void Dispose()
    {
        Automation.UnregisterAllEvents();
        Automation.Dispose();
    }
    ~UiAutomation()
    {
        Dispose();
    }
}