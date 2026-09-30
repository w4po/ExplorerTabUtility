using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;

namespace ExplorerTabUtility.UI.Views.Controls
{
    class BookmarkMenu : Menu
    {
        #region 事件
        public delegate void BookmarkEventHandler(BookmarkMenuInfo info, BookmarkInfo bookmark, BookmarkAction action);
        public event BookmarkEventHandler? BookmarkHandle;

        public delegate void FolderEventHandler(BookmarkMenuInfo info, FolderInfo folder, BookmarkAction action);
        public event FolderEventHandler? FolderHandle;
        #endregion

        private BookmarkMenuInfo allBookmark;
        private ObservableCollection<BookmarkMenuInfo> mainBookmarks;
        private BookmarkMenuInfo overflowBookmark;
        private BookmarkMenuInfo otherBookmark;
        private BookmarkMenuSeparatorInfo separator;
        private ObservableCollection<BaseBookmarkMenuInfo> bindBookmarks;

        public BookmarkMenu()
        {
            PreviewMouseLeftButtonDown += BookmarkMenu_PreviewMouseLeftButtonDown;
            allBookmark = new BookmarkMenuInfo(BookmarkManager.Folder, -1, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, true);
            mainBookmarks = new ObservableCollection<BookmarkMenuInfo>();
            overflowBookmark = new BookmarkMenuInfo(BookmarkManager.OverflowFolder, 0, null) { IsDockRight = true };
            otherBookmark = new BookmarkMenuInfo(BookmarkManager.OtherFolder, 0, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, false) { IsDockRight = true };
            separator = new BookmarkMenuSeparatorInfo { IsDockRight = true };
            bindBookmarks = new ObservableCollection<BaseBookmarkMenuInfo>();
            ItemsSource = bindBookmarks;
        }

        private void BookmarkMenu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source) return;

            var item = VisualTreeHelperEx.GetParent<MenuItem>(source);
            if (item == null) return;
            if (item.DataContext is not BookmarkMenuInfo info) return;

            if (info.IsFolder)
            {
                if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                {
                    FolderHandle?.Invoke(info, info.CurrentFolder, BookmarkAction.OpenInNewTab);
                    e.Handled = true;
                }
                else if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Shift))
                {
                    FolderHandle?.Invoke(info, info.CurrentFolder, BookmarkAction.OpenInNewWindow);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 初始化布局
        /// </summary>
        public void InitLayout()
        {
            UpdateMenuLayout();
        }

        private void UpdateMenuLayout()
        {
            var allBookmarks = allBookmark.Children;

            //清空主书签和溢出书签，设置分隔符是否显示
            mainBookmarks.Clear();
            overflowBookmark.Children.Clear();

            //所有书签数量为0直接返回
            if (allBookmarks.Count == 0) return;

            //初始可用宽度是控件的宽度
            var availableWidth = Width;

            //其他书签有子集合，就会显示，减去其宽度
            if (otherBookmark.Children.Count > 0)
            {
                if (otherBookmark.Width == 0)
                {
                    otherBookmark.Width = GetItemWidth(otherBookmark.DisplayName);
                }

                availableWidth -= otherBookmark.Width;
                separator.SetIsShowSubIcon(true);
            }
            else
            {
                separator.SetIsShowSubIcon(false);
            }

            //减去溢出书签的宽度
            if (overflowBookmark.Width == 0)
            {
                overflowBookmark.Width = GetItemWidth(overflowBookmark.DisplayName);
            }
            availableWidth -= overflowBookmark.Width;

            //主书签
            int index;
            BookmarkMenuInfo item;
            for (index = 0; index < allBookmarks.Count; index++)
            {
                item = allBookmarks[index];
                if (item.Width == 0)
                {
                    item.Width = GetItemWidth(item.DisplayName);
                }

                if (availableWidth < item.Width)
                {
                    break;
                }

                item.FirstLevel = true;
                item.Parent = allBookmark;
                item.PlacementMode = PlacementMode.Bottom;
                mainBookmarks.Add(item);
                availableWidth -= item.Width;
            }

            //溢出书签
            for (; index < allBookmarks.Count; index++)
            {
                item = allBookmarks[index];
                item.FirstLevel = false;
                item.Parent = overflowBookmark;
                item.PlacementMode = PlacementMode.Right;
                overflowBookmark.Children.Add(item);
            }

            bindBookmarks.Clear();
            bindBookmarks.Add(otherBookmark);
            bindBookmarks.Add(separator);
            bindBookmarks.Add(overflowBookmark);
            foreach (var temp in mainBookmarks)
            {
                bindBookmarks.Add(temp);
            }
        }

        private int GetItemWidth(string name)
        {
            var width = 0;

            //名字的宽度
            var byteWith = 10;
            var byteCount = GetByteCount(name);
            width += byteCount * byteWith;

            //图标的宽度
            var iconWidth = 30;
            width += iconWidth;

            return width;
        }

        /// <summary>
        /// 计算字符数，大于255当汉字两个字符
        /// </summary>
        /// <returns></returns>
        private int GetByteCount(string str)
        {
            int count = 0;
            foreach (char c in str)
            {
                count += c > 255 ? 2 : 1;
            }
            return count;
        }

        private void BookmarkClickAction(BookmarkMenuInfo info, BookmarkInfo bookmark)
        {
            BookmarkAction action;
            if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
            {
                action = BookmarkAction.OpenInNewTab;
            }
            else if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Shift))
            {
                action = BookmarkAction.OpenInNewWindow;
            }
            else
            {
                action = BookmarkAction.OpenInCurrentTab;
            }

            BookmarkHandle?.Invoke(info, bookmark, action);
        }

        private void BookmarkMenuClickAction(BookmarkMenuInfo info, BookmarkInfo bookmark, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.ManageBookmarks:
                case BookmarkAction.NewBookmark:
                case BookmarkAction.NewFolder:
                case BookmarkAction.OpenInCurrentTab:
                case BookmarkAction.OpenInNewTab:
                case BookmarkAction.OpenInNewWindow:
                case BookmarkAction.Edit:
                    BookmarkHandle?.Invoke(info, bookmark, action);
                    break;
                case BookmarkAction.Delete:
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    BookmarkManager.Delete(info.Parent.CurrentFolder, bookmark);
#pragma warning restore CS8602 // 解引用可能出现空引用。
                    BookmarkManager.SaveConfig();

                    var needUpdateLayout = NeedUpdateMenuLayout(info, Guid.Empty);
                    info.Parent.Delete(info);
                    Delete(needUpdateLayout, info);
                    break;
            }
        }

        private void FolderMenuClickAction(BookmarkMenuInfo info, FolderInfo folder, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.ManageBookmarks:
                case BookmarkAction.OpenInNewTab:
                case BookmarkAction.OpenInNewWindow:
                case BookmarkAction.NewBookmark:
                case BookmarkAction.NewFolder:
                case BookmarkAction.Rename:
                    FolderHandle?.Invoke(info, folder, action);
                    break;
                case BookmarkAction.Delete:
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    BookmarkManager.Delete(info.Parent.CurrentFolder, folder);
#pragma warning restore CS8602 // 解引用可能出现空引用。
                    BookmarkManager.SaveConfig();

                    var needUpdateLayout = NeedUpdateMenuLayout(info, Guid.Empty);
                    info.Parent.Delete(info);
                    Delete(needUpdateLayout, info);
                    break;
            }
        }

        private void Delete(bool needUpdateLayout, BookmarkMenuInfo info)
        {
            allBookmark.Children.Remove(info);
            mainBookmarks.Remove(info);

            if (needUpdateLayout)
            {
                UpdateMenuLayout();
            }
        }

        public void RenameFolder(BookmarkMenuInfo info, string newName)
        {
            info.Name = newName;

            if (NeedUpdateMenuLayout(info, Guid.Empty))
            {
                info.Width = 0;
                UpdateMenuLayout();
            }
        }

        public void EditBookmark(BookmarkMenuInfo info, BookmarkInfo bookmark)
        {
            info.Name = bookmark.Name;

            if (info.Parent == null) throw new ArgumentNullException(nameof(info.Parent));

            var needUpdateLayout = NeedUpdateMenuLayout(info, bookmark.ParentId);
            if (info.GetParentId() != bookmark.ParentId)
            {
                info.Parent.Delete(info);

                var newParent = SearchFolder(bookmark.ParentId);
                if (newParent != null)
                {
                    newParent.Add(info);
                }
            }

            if (needUpdateLayout)
            {
                info.Width = 0;
                UpdateMenuLayout();
            }
        }

        public void Refresh()
        {
            allBookmark = new BookmarkMenuInfo(BookmarkManager.Folder, -1, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, true);
            otherBookmark = new BookmarkMenuInfo(BookmarkManager.OtherFolder, 0, null, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, false) { IsDockRight = true };

            UpdateMenuLayout();
        }

        public void NewBookmark(BookmarkInfo bookmark)
        {
            var parent = SearchFolder(bookmark.ParentId);
            if (parent != null)
            {
                var info = new BookmarkMenuInfo(bookmark, parent.Level + 1, parent, BookmarkClickAction, BookmarkMenuClickAction);
                parent.Add(info);

                var needUpdateLayout = NeedUpdateMenuLayout(info, bookmark.ParentId);
                if (needUpdateLayout)
                {
                    UpdateMenuLayout();
                }
            }
        }

        public void NewFolder(BookmarkMenuInfo info, FolderInfo folder)
        {
            var parentId = info.GetCurrentFolderId();
            var newInfo = new BookmarkMenuInfo(folder, info.Level + 1, info, BookmarkClickAction, BookmarkMenuClickAction, FolderMenuClickAction, true);

            if (parentId == BookmarkManager.Folder.Id)
            {
                allBookmark.Add(newInfo);
            }
            else
            {
                info.Add(newInfo);
            }

            var needUpdateLayout = NeedUpdateMenuLayout(info, parentId);
            if (needUpdateLayout)
            {
                UpdateMenuLayout();
            }
        }

        private BookmarkMenuInfo? SearchFolder(Guid searchId)
        {
            var searchData = new List<BookmarkMenuInfo>() { allBookmark, otherBookmark };
            return BookmarkMenuInfo.SearchFolder(searchData, searchId);
        }

        private bool NeedUpdateMenuLayout(BookmarkMenuInfo info, Guid newParentId)
        {
            //简单判断：涉及主书签和其他书签就更新菜单布局
            if (mainBookmarks.Contains(info) || otherBookmark.Children.Contains(info)) return true;
            if (newParentId == Guid.Empty) return false;

            return newParentId == BookmarkManager.Folder.Id || newParentId == BookmarkManager.OtherFolder.Id;
        }
    }
}
