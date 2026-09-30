using System.ComponentModel;
using System.Text.Json.Serialization;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.Models;

[JsonConverter(typeof(HotKeyActionJsonConverter))]
public enum HotKeyAction
{
    [Description("Open a new tab/window with the specified location.")]
    Open,
    [Description("Duplicate the current tab.")]
    Duplicate,
    [Description("Reopen the last closed location.")]
    ReopenClosed,
    [Description("Open tab search popup to find and switch between tabs.")]
    TabSearch,
    [Description("Navigate back.")]
    NavigateBack,
    [Description("Navigate up.")]
    NavigateUp,
    [Description("Navigate forward.")]
    NavigateForward,
    [Description("Mark the window that will receive the new tabs.")]
    SetTargetWindow,
    [Description("Toggle the window hook.")]
    ToggleWinHook,
    [Description("Toggle the reuse tabs option.")]
    ToggleReuseTabs,
    [Description("Show/Hide the app.")]
    ToggleVisibility,
    [Description("Detach the current tab.")]
    DetachTab,
    [Description("Snap the current window to the right.")]
    SnapRight,
    [Description("Snap the current window to the left.")]
    SnapLeft,
    [Description("Snap the current window to the top.")]
    SnapUp,
    [Description("Snap the current window to the bottom.")]
    SnapDown,
    [Description("Open bookmark save popup to save tab location as bookmark.")]
    BookmarkSave,
    [Description("Open bookmark manage popup to manage bookmarks.")]
    BookmarkManage,
    [Description("Open bookmark navigate popup to open a new tab/window with the bookmark location.")]
    BookmarkNavigate,
}