using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Languages.Manager;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.WinAPI;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.UI.Views.Controls
{
    internal class BookmarkTreeView : TreeView
    {
        #region 事件
        public delegate void BookmarkEventHandler(BookmarkTreeViewInfo info, BookmarkInfo bookmark, BookmarkAction action);
        public event BookmarkEventHandler? BookmarkHandle;

        public delegate void FolderEventHandler(BookmarkTreeViewInfo info, FolderInfo folder, BookmarkAction action);
        public event FolderEventHandler? FolderHandle;
        #endregion

        /// <summary>
        /// 是否有保存
        /// </summary>
        public bool HaveSave { get; private set; }

        private bool withBookmark;

        public BookmarkTreeView()
        {
            SetupEventHandlers();
        }

        #region 事件注册
        private void SetupEventHandlers()
        {
            KeyDown += BookmarkTreeView_KeyDown;
        }

        private void BookmarkTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    Rename();
                    break;
                case Key.Delete:
                    Delete();
                    break;
                case Key.T:
                    HandleCtrlEvent(BookmarkAction.OpenInNewTab);
                    break;
                case Key.W:
                    HandleCtrlEvent(BookmarkAction.OpenInNewWindow);
                    break;
                case Key.X:
                    HandleCtrlEvent(BookmarkAction.Cut, t => t.FirstLevel == false);
                    break;
                case Key.C:
                    HandleCtrlEvent(BookmarkAction.Copy);
                    break;
                case Key.V:
                    HandleCtrlEvent(BookmarkAction.Paste, t => BookmarkManager.ClipboardManager.TreeViewCanPaste);
                    break;
                case Key.N:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        var info = (BookmarkTreeViewInfo)SelectedItem;
                        if (info != null)
                        {
                            AddFolder(out _);
                        }
                    }
                    break;
            }
        }

        private void HandleCtrlEvent(BookmarkAction action) => HandleCtrlEvent(action, t => true);

        private void HandleCtrlEvent(BookmarkAction action, Func<BookmarkTreeViewInfo, bool> condition)
        {
            if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
            {
                var info = (BookmarkTreeViewInfo)SelectedItem;
                if (info != null && condition(info))
                {
                    FolderHandle?.Invoke(info, info.CurrentFolder, action);
                }
            }
        }
        #endregion

        #region 功能
        public void OnLangeuageChanged()
        {
            var datas = (ObservableCollection<BookmarkTreeViewInfo>)ItemsSource;
            if (datas == null) return;

            foreach (var item in datas)
            {
                if (item.Id == BookmarkManager.Folder.Id)
                {
                    item.UpdateFolderName(LangeuageHelper.Instance.LanguageFields.BookmarkBar);
                }
                else if (item.Id == BookmarkManager.OtherFolder.Id)
                {
                    item.UpdateFolderName(LangeuageHelper.Instance.LanguageFields.OtherBookmark);
                }
            }
        }

        /// <summary>
        /// 复制文件夹信息
        /// <para>树形结构转化而来</para>
        /// </summary>
        /// <returns></returns>
        public List<FolderInfo> CopyFolderInfos()
        {
            var list = new List<FolderInfo>();
            var datas = (ObservableCollection<BookmarkTreeViewInfo>)ItemsSource;
            foreach (var item in datas)
            {
                list.Add(item.CopyFolderInfo(true));
            }
            return list;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<BookmarkTreeViewInfo> Search(string key)
        {
            var datas = (ObservableCollection<BookmarkTreeViewInfo>)ItemsSource;
            foreach (var item in datas)
            {
                foreach (var temp in item.Search(key))
                {
                    yield return temp;
                }
            }
        }

        /// <summary>
        /// 重命名
        /// </summary>
        public void Rename()
        {
            var info = (BookmarkTreeViewInfo)SelectedItem;
            if (info == null || info.Parent == null) return;

            info.IsEditMode = true;
        }

        public void Delete()
        {
            var info = (BookmarkTreeViewInfo)SelectedItem;
            if (info == null || info.Parent == null) return;

            if (withBookmark)
            {
                Delete([info]);
                return;
            }

            BookmarkManager.Delete(info.Parent.CurrentFolder, info.CurrentFolder);
            info.Parent.Delete(info);
            HaveSave = true;
        }

        public void Delete(List<BookmarkTreeViewInfo> infos)
        {
            var deleteFolderIds = infos.SelectMany(t => t.CopyFolderInfo(false).GetFolderIds()).ToList();
            BookmarkManager.LastSaveFoldersDelete(deleteFolderIds);

            var datas = (ObservableCollection<BookmarkTreeViewInfo>)ItemsSource;
            foreach (var item in datas)
            {
                item.Delete(infos);
            }
        }

        public void Cut(List<BookmarkTreeViewInfo> infos)
        {
            foreach (var item in infos)
            {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                item.Parent.Delete(item);
#pragma warning restore CS8602 // 解引用可能出现空引用。
            }
        }

        /// <summary>
        /// 获取选中项的SaveFolderItem
        /// </summary>
        /// <returns></returns>
        public SaveFolderItem? GetSaveFolderItem()
        {
            return ((BookmarkTreeViewInfo)SelectedItem)?.SaveFolderItem;
        }

        /// <summary>
        /// 选中项新建文件夹，返回是否成功
        /// </summary>
        /// <param name="errorMsg">失败时的消息</param>
        /// <returns></returns>
        public bool AddFolder(out string errorMsg)
        {
            var selected = (BookmarkTreeViewInfo)SelectedItem;
            if (selected == null)
            {
                errorMsg = LangeuageHelper.Instance.LanguageFields.PleaseSelectNewFolderLocation;
                return false;
            }

            var newFolder = new FolderInfo(Guid.Empty, LangeuageHelper.Instance.LanguageFields.NewFolder);
            var newInfo = new BookmarkTreeViewInfo(newFolder, selected.Level + 1, selected, false, FolderMenuClickAction)
            {
                IsEditMode = true
            };
            selected.Add(newInfo);
            selected.IsExpanded = true;
            errorMsg = string.Empty;
            return true;
        }

        /// <summary>
        /// 新建文件夹，返回是否成功
        /// </summary>
        /// <param name="newInfo"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool AddFolder(BookmarkTreeViewInfo newInfo, out string errorMsg)
        {
#pragma warning disable CS8602 // 解引用可能出现空引用。
            if (BookmarkManager.Save(newInfo.Parent.Id, newInfo.CurrentFolder, newInfo.Name, false))
#pragma warning restore CS8602 // 解引用可能出现空引用。
            {
                HaveSave = true;
                newInfo.UpdateFolder();
                newInfo.IsEditMode = false;

                errorMsg = string.Empty;
                return true;
            }
            else
            {
                errorMsg = LangeuageHelper.Instance.LanguageFields.SaveFailed;
                return false;
            }
        }
        #endregion

        #region 菜单事件
        private void BookmarkMenuClickAction(BookmarkTreeViewInfo info, BookmarkInfo bookmark, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.Edit:
                case BookmarkAction.Delete:
                case BookmarkAction.OpenInCurrentTab:
                case BookmarkAction.OpenInNewTab:
                case BookmarkAction.OpenInNewWindow:
                case BookmarkAction.Cut:
                case BookmarkAction.Copy:
                case BookmarkAction.Paste:
                case BookmarkAction.ShowInFolder:
                    BookmarkHandle?.Invoke(info, bookmark, action);
                    break;
            }
        }

        private void FolderMenuClickAction(BookmarkTreeViewInfo info, FolderInfo folder, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.NewFolder:
                    AddFolder(out _);
                    break;
                case BookmarkAction.Delete:
                case BookmarkAction.Rename:
                case BookmarkAction.OpenInNewTab:
                case BookmarkAction.OpenInNewWindow:
                case BookmarkAction.Cut:
                case BookmarkAction.Copy:
                case BookmarkAction.Paste:
                case BookmarkAction.ShowInFolder:
                    FolderHandle?.Invoke(info, folder, action);
                    break;
            }
        }
        #endregion

        #region 设置数据源
        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="folders">数据源</param>
        /// <param name="expandedId">要显示的id</param>
        /// <param name="withBookmark">是否带书签</param>
        public void SetItemsSource(IReadOnlyCollection<FolderInfo> folders, Guid expandedId, bool withBookmark)
        {
            this.withBookmark = withBookmark;
            var datas = new ObservableCollection<BookmarkTreeViewInfo>();
            if (withBookmark)
            {
                int index = 0;
                foreach (var item in folders)
                {
                    datas.Add(CreateItemWithBookmark(item, 0, null, index == 0, expandedId, true));
                    index++;
                }
            }
            else
            {
                int index = 0;
                foreach (var item in folders)
                {
                    datas.Add(CreateItemWithOutBookmark(item, 0, null, index == 0, expandedId, false));
                    index++;
                }
            }
            ItemsSource = datas;
        }

        /// <summary>
        /// 创建带书签的数据项
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="isSpecil"></param>
        /// <param name="expandedId"></param>
        /// <param name="expandedSelf"></param>
        /// <returns></returns>
        private BookmarkTreeViewInfo CreateItemWithBookmark(FolderInfo folder, int level, BookmarkTreeViewInfo? parent, bool isSpecil, Guid expandedId, bool expandedSelf)
        {
            var result = new BookmarkTreeViewInfo(folder, level, parent, isSpecil, FolderMenuClickAction);
            if (folder.Items.Count > 0)
            {
                level++;
                foreach (var item in folder.Items)
                {
                    if (item is FolderInfo folderInfo)
                    {
                        result.Add(CreateItemWithBookmark(folderInfo, level, result, false, expandedId, expandedSelf));
                    }
                    else if (item is BookmarkInfo bookmarkInfo)
                    {
                        result.Add(new BookmarkTreeViewInfo(bookmarkInfo, level, result, BookmarkMenuClickAction));
                    }
                }
            }
            if (folder.Id == expandedId)
            {
                result.IsSelected = true;
                Expanded(result, expandedSelf);
            }

            return result;
        }

        /// <summary>
        /// 创建不带书签的数据项
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="level"></param>
        /// <param name="parent"></param>
        /// <param name="isSpecil"></param>
        /// <param name="expandedId"></param>
        /// <param name="expandedSelf"></param>
        /// <returns></returns>
        private BookmarkTreeViewInfo CreateItemWithOutBookmark(FolderInfo folder, int level, BookmarkTreeViewInfo? parent, bool isSpecil, Guid expandedId, bool expandedSelf)
        {
            var result = new BookmarkTreeViewInfo(folder, level, parent, isSpecil, FolderMenuClickAction);
            if (folder.Items.Count > 0)
            {
                level++;
                var folders = folder.Items.OfType<FolderInfo>();
                foreach (var folderInfo in folders)
                {
                    result.Add(CreateItemWithOutBookmark(folderInfo, level, result, false, expandedId, expandedSelf));
                }
            }
            if (folder.Id == expandedId)
            {
                result.IsSelected = true;
                Expanded(result, expandedSelf);
            }

            return result;
        }

        public static void Expanded(BookmarkTreeViewInfo item, bool expandedSelf)
        {
            if (expandedSelf)
            {
                item.IsExpanded = true;
            }

            var parent = item.Parent;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent;
            }
        }
        #endregion
    }
}
