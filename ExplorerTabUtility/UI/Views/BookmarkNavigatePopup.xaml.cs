using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Languages.Manager;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;
using ExplorerTabUtility.WinAPI;
using HKey = H.Hooks.Key;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkNavigatePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkNavigatePopup : BaseBookmarkWindow
    {
        private Key[] hotKeys;

        public BookmarkNavigatePopup(ExplorerWatcher explorerWatcher, nint windowHandle) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            var margin = 10;
            var currentScreenWorkingArea = ScreenHelper.MouseCurrentScreen.WorkingArea;
            Top = margin + currentScreenWorkingArea.Top;
            Left = margin + currentScreenWorkingArea.Left;
            BookmarkMenu.Width = Width = currentScreenWorkingArea.Width - margin * 2;
            BookmarkMenu.InitLayout();

            hotKeys = GetBookmarkManagerHotKey();
            SetupEventHandlers();
        }

        private Key[] GetBookmarkManagerHotKey()
        {
            var profiles = JsonSerializer.Deserialize<List<HotKeyProfile>>(SettingsManager.HotKeyProfiles);
            var profile = profiles?.FirstOrDefault(t => t.Action == HotKeyAction.BookmarkManage);
            if (profile != null && profile.HotKeys?.Length > 0)
            {
                var keys = profile.HotKeys.Select(t => TryConvert(t, out var key) ? key : Key.None).ToArray();
                if (keys.All(t => t != Key.None))
                {
                    BookmarkManager.ClipboardManager.ManageHotKey = profile.HotKeys.HotKeysToString().Replace(" ", "");
                    return keys;
                }
            }

            BookmarkManager.ClipboardManager.ManageHotKey = "Ctrl+B";
            return [Key.LeftCtrl, Key.B];
        }

        private bool TryConvert(HKey key, out Key result)
        {
            switch (key)
            {
                case HKey.Ctrl:
                case HKey.LeftCtrl:
                case HKey.RightCtrl:
                    result = Key.LeftCtrl;
                    return true;
                case HKey.Shift:
                case HKey.LeftShift:
                case HKey.RightShift:
                    result = Key.LeftShift;
                    return true;
                case HKey.Alt:
                case HKey.LeftAlt:
                case HKey.RightAlt:
                    result = Key.LeftAlt;
                    return true;
                case HKey.LWin:
                    result = Key.LWin;
                    return true;
                case HKey.RWin:
                    result = Key.RWin;
                    return true;
            }
            return Enum.TryParse(key.ToString(), out result);
        }

        private bool IsPressedBookmarkManagerHotKey(Key key)
        {
            foreach (var item in hotKeys)
            {
                VirtualKey? virtualKey = null;
                switch (item)
                {
                    case Key.LeftCtrl:
                        virtualKey = VirtualKey.Control;
                        break;
                    case Key.LeftShift:
                        virtualKey = VirtualKey.Shift;
                        break;
                    case Key.LeftAlt:
                        virtualKey = VirtualKey.Alt;
                        break;
                    case Key.LWin:
                        virtualKey = VirtualKey.LWin;
                        break;
                    case Key.RWin:
                        virtualKey = VirtualKey.RWin;
                        break;
                    default:
                        if (item != key)
                        {
                            return false;
                        }
                        break;
                }
                if (virtualKey != null && KeyboardSimulator.IsKeyPressed((int)virtualKey) == false)
                {
                    return false;
                }
            }
            return true;
        }

        private void ShowBookmarkManager()
        {
            var popup = new BookmarkManagePopup(explorerWatcher, windowHandle, true);
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkMenu.Refresh();
            }
            ExitDialog();
        }

        private void NewBookmark(Guid parentId)
        {
            var bookmark = CreateCurrentLocationBookmark();
            var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, bookmark, parentId);
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkMenu.NewBookmark(bookmark);
            }
            ExitDialog();
        }

        private void NewFolder(BookmarkMenuInfo info)
        {
            var newFolder = new FolderInfo(Guid.Empty, LangeuageHelper.Instance.LanguageFields.NewFolder);
            var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, newFolder, info.GetCurrentFolderId());
            EntryDialog();
            if (popup.ShowDialog() == true)
            {
                BookmarkMenu.NewFolder(info, newFolder);
            }
            ExitDialog();
        }

        private async void OpenFolderBookmark(BookmarkMenuInfo info, BookmarkAction action)
        {
            var infos = info.GetBookmarkInfos().ToList();
            if (infos.Count == 0) return;

            if (action == BookmarkAction.OpenInNewTab)
            {
                foreach (var item in infos)
                {
                    await explorerWatcher.Open(item.Location, true, windowHandle, inCurrentTab: false);
                }
                return;
            }

            var newWindowHandle = await explorerWatcher.Open(infos.First().Location, false, windowHandle);
            if (newWindowHandle != 0 && infos.Count > 1)
            {
                foreach (var item in infos)
                {
                    await explorerWatcher.Open(item.Location, true, newWindowHandle, inCurrentTab: false);
                }
            }
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            Deactivated += BookmarkNavigatePopup_Deactivated;
            KeyDown += BookmarkNavigatePopup_KeyDown;

            BookmarkMenu.BookmarkHandle += BookmarkMenu_BookmarkHandle;
            BookmarkMenu.FolderHandle += BookmarkMenu_FolderHandle;
        }

        private void BookmarkMenu_FolderHandle(BookmarkMenuInfo info, FolderInfo folder, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.Rename:
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, folder, Guid.Empty);
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkMenu.RenameFolder(info, folder.Name);
                    }
                    ExitDialog();
                    break;
                case BookmarkAction.OpenInNewTab:
                case BookmarkAction.OpenInNewWindow:
                    OpenFolderBookmark(info, action);
                    break;
                case BookmarkAction.ManageBookmarks:
                    ShowBookmarkManager();
                    break;
                case BookmarkAction.NewBookmark:
                    NewBookmark(folder.Id);
                    break;
                case BookmarkAction.NewFolder:
                    NewFolder(info);
                    break;
            }
        }

        private async void BookmarkMenu_BookmarkHandle(BookmarkMenuInfo info, BookmarkInfo bookmark, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.OpenInCurrentTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: true);
                    CloseWindow();
                    break;
                case BookmarkAction.OpenInNewTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: false);
                    CloseWindow();
                    break;
                case BookmarkAction.OpenInNewWindow:
                    await explorerWatcher.Open(bookmark.Location, false, windowHandle);
                    CloseWindow();
                    break;
                case BookmarkAction.Edit:
                    var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, bookmark, info.GetParentId());
                    EntryDialog();
                    if (popup.ShowDialog() == true)
                    {
                        BookmarkMenu.EditBookmark(info, bookmark);
                    }
                    ExitDialog();
                    break;
                case BookmarkAction.ManageBookmarks:
                    ShowBookmarkManager();
                    break;
                case BookmarkAction.NewBookmark:
                    NewBookmark(info.GetParentId());
                    break;
                case BookmarkAction.NewFolder:
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    NewFolder(info.Parent);
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                    break;
            }
        }

        private void BookmarkNavigatePopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
            else if (IsPressedBookmarkManagerHotKey(e.Key))
            {
                ShowBookmarkManager();
            }
        }

        private void BookmarkNavigatePopup_Deactivated(object? sender, System.EventArgs e)
        {
            if (CanClose()) CloseWindow();
        }
        #endregion
    }
}
