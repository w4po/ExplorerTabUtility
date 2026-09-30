using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.UI.Commands;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.Models
{
    public class BookmarkTreeViewInfo : BindableBase
    {
        #region 属性
        private bool isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        private bool isExpanded;
        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        private bool isEditMode;
        /// <summary>
        /// 是否编辑模式
        /// </summary>
        public bool IsEditMode
        {
            get { return isEditMode; }
            set { SetProperty(ref isEditMode, value); }
        }

        private string icon;
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon
        {
            get { return icon; }
            set { SetProperty(ref icon, value); }
        }

        private string expandedIcon;
        /// <summary>
        /// 展开的图标
        /// </summary>
        public string ExpandedIcon
        {
            get { return expandedIcon; }
            set { SetProperty(ref expandedIcon, value); }
        }

        private ClickMode clickMode;
        /// <summary>
        /// 展开模式
        /// </summary>
        public ClickMode ClickMode
        {
            get { return clickMode; }
            set { SetProperty(ref clickMode, value); }
        }

        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private ObservableCollection<BookmarkTreeViewInfo> children;
        /// <summary>
        /// 子集合
        /// </summary>
        public ObservableCollection<BookmarkTreeViewInfo> Children
        {
            get { return children; }
            set { SetProperty(ref children, value); }
        }

        private BookmarkTreeViewInfo? parent;
        /// <summary>
        /// 父节点
        /// </summary>
        public BookmarkTreeViewInfo? Parent
        {
            get { return parent; }
            private set { SetProperty(ref parent, value); }
        }

        private int level;
        /// <summary>
        /// 节点等级
        /// </summary>
        public int Level
        {
            get { return level; }
            set
            {
                SetProperty(ref level, value);
                RaisePropertyChanged(nameof(FirstLevel));
            }
        }

        /// <summary>
        /// 保存路径
        /// </summary>
        public SaveFolderItem SaveFolderItem { get; }

        /// <summary>
        /// 当前文件夹
        /// </summary>
        public FolderInfo CurrentFolder { get; private set; }

        /// <summary>
        /// 当前书签
        /// </summary>
        public BookmarkInfo CurrentBookmark { get; }

        /// <summary>
        /// 是否第一层节点
        /// </summary>
        public bool FirstLevel => Level == 0;

        /// <summary>
        /// 是否需要保存
        /// </summary>
        public bool IsNeedSave => IsAdd || name != oldName;

        /// <summary>
        /// 是否是新增
        /// </summary>
        public bool IsAdd => Id == Guid.Empty;

        /// <summary>
        /// 主键
        /// </summary>
        public Guid Id => SaveFolderItem.Key;

        /// <summary>
        /// 是否显示
        /// </summary>
        public Visibility Visibility { get; set; }

        /// <summary>
        /// 是否有显示的子项
        /// </summary>
        public bool HasVisibilityItems => children.Any(t => t.Visibility == Visibility.Visible);

        /// <summary>
        /// 是否是文件夹
        /// </summary>
        public bool IsFolder { get; }

        /// <summary>
        /// 菜单点击事件
        /// </summary>
        public ICommand MenuClickCommand { get; }
        #endregion

        private readonly string oldName;
        private readonly ObservableCollection<BookmarkTreeViewInfo> emptyChildren = [];
        private readonly Action<BookmarkTreeViewInfo, FolderInfo, BookmarkAction>? folderInfoMenuClickAction;
        private readonly Action<BookmarkTreeViewInfo, BookmarkInfo, BookmarkAction>? bookmarkInfoMenuClickAction;

        public BookmarkTreeViewInfo(BookmarkInfo bookmarkInfo, int level, BookmarkTreeViewInfo? parent, Action<BookmarkTreeViewInfo, BookmarkInfo, BookmarkAction> menuClickAction)
        {
            Parent = parent;
            SaveFolderItem = new SaveFolderItem(bookmarkInfo.Id, bookmarkInfo.Name, bookmarkInfo.Location);
            CurrentFolder = FolderInfo.Empty;
            CurrentBookmark = bookmarkInfo;
            Level = level;
            Visibility = Visibility.Collapsed;
            IsFolder = false;

            children = emptyChildren;
            icon = GetIcon(true, false, false);
            expandedIcon = icon;
            oldName = name = bookmarkInfo.Name;

            bookmarkInfoMenuClickAction = menuClickAction;
            MenuClickCommand = new RelayCommand((args) =>
            {
                if (args is BookmarkAction action)
                {
                    menuClickAction.Invoke(this, CurrentBookmark, action);
                }
            });
        }

        public BookmarkTreeViewInfo(FolderInfo folderInfo, int level, BookmarkTreeViewInfo? parent, bool isSpecil, Action<BookmarkTreeViewInfo, FolderInfo, BookmarkAction> menuClickAction)
        {
            Parent = parent;
            SaveFolderItem = new SaveFolderItem(folderInfo.Id, folderInfo.Name);
            CurrentFolder = folderInfo;
            CurrentBookmark = BookmarkInfo.Empty;
            Level = level;
            Visibility = Visibility.Visible;
            IsFolder = true;

            children = [];
            icon = GetIcon(false, false, isSpecil);
            expandedIcon = GetIcon(false, true, isSpecil);
            oldName = name = folderInfo.Name;

            folderInfoMenuClickAction = menuClickAction;
            MenuClickCommand = new RelayCommand((args) =>
            {
                if (args is BookmarkAction action)
                {
                    menuClickAction.Invoke(this, CurrentFolder, action);
                }
            });
        }

        private BookmarkTreeViewInfo(BookmarkTreeViewInfo target, int parentLevel, BookmarkInfo bookmarkInfo)
        {
            Parent = target.parent;
            SaveFolderItem = new SaveFolderItem(bookmarkInfo.Id, bookmarkInfo.Name, bookmarkInfo.Location);
            CurrentFolder = FolderInfo.Empty;
            CurrentBookmark = bookmarkInfo;
            Level = parentLevel + 1;
            Visibility = Visibility.Collapsed;
            IsFolder = false;

            children = emptyChildren;
            icon = target.icon;
            expandedIcon = target.expandedIcon;
            oldName = name = bookmarkInfo.Name;

            bookmarkInfoMenuClickAction = target.bookmarkInfoMenuClickAction;
            MenuClickCommand = new RelayCommand((args) =>
            {
                if (args is BookmarkAction action)
                {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    bookmarkInfoMenuClickAction.Invoke(this, CurrentBookmark, action);
#pragma warning restore CS8602 // 解引用可能出现空引用。
                }
            });
        }

        private BookmarkTreeViewInfo(BookmarkTreeViewInfo target, int parentLevel, FolderInfo folderInfo, Func<Guid> guidFactory)
        {
            Parent = target.parent;
            SaveFolderItem = new SaveFolderItem(folderInfo.Id, folderInfo.Name);
            CurrentFolder = folderInfo;
            CurrentBookmark = BookmarkInfo.Empty;
            Level = parentLevel + 1;
            Visibility = Visibility.Visible;
            IsFolder = true;

            children = [];
            foreach (var item in target.children)
            {
                children.Add(item.Copy(guidFactory, Level));
            }

            icon = target.icon;
            expandedIcon = target.expandedIcon;
            oldName = name = folderInfo.Name;

            folderInfoMenuClickAction = target.folderInfoMenuClickAction;
            MenuClickCommand = new RelayCommand((args) =>
            {
                if (args is BookmarkAction action)
                {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    folderInfoMenuClickAction.Invoke(this, CurrentFolder, action);
#pragma warning restore CS8602 // 解引用可能出现空引用。
                }
            });
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<BookmarkTreeViewInfo> Search(string key)
        {
            if (CurrentBookmark != BookmarkInfo.Empty)
            {
#if NET481
                if (CurrentBookmark.Name.IndexOf(key, StringComparison.OrdinalIgnoreCase) != -1 || CurrentBookmark.Location.IndexOf(key, StringComparison.OrdinalIgnoreCase) != -1)
#elif NET9_0
                if (CurrentBookmark.Name.Contains(key, StringComparison.OrdinalIgnoreCase) || CurrentBookmark.Location.Contains(key, StringComparison.OrdinalIgnoreCase))
#endif
                {
                    yield return this;
                }
            }
            else
            {
                if (BookmarkManager.IsMainOrOtherFolder(CurrentFolder) == false)
                {
#if NET481
                    if (CurrentFolder.Name.IndexOf(key, StringComparison.OrdinalIgnoreCase) != -1)
#elif NET9_0
                    if (CurrentFolder.Name.Contains(key, StringComparison.OrdinalIgnoreCase))
#endif
                    {
                        yield return this;
                    }
                }

                foreach (var item in children)
                {
                    foreach (var temp in item.Search(key))
                    {
                        yield return temp;
                    }
                }
            }
        }

        /// <summary>
        /// 获取书签，文件夹的话只获取当前文件夹的，不会递归子文件夹
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BookmarkInfo> GetBookmarkInfos()
        {
            if (CurrentBookmark != BookmarkInfo.Empty)
            {
                yield return CurrentBookmark;
            }
            else
            {
                foreach (var item in children)
                {
                    if (item.CurrentBookmark != BookmarkInfo.Empty)
                    {
                        yield return item.CurrentBookmark;
                    }
                }
            }
        }

        /// <summary>
        /// 复制文件夹信息
        /// </summary>
        /// <param name="copyBookmark">是否复制书签</param>
        /// <returns></returns>
        public FolderInfo CopyFolderInfo(bool copyBookmark)
        {
            var folder = CurrentFolder.Copy();
            foreach (var item in children)
            {
                if (item.CurrentFolder != FolderInfo.Empty)
                {
                    folder.Add(item.CopyFolderInfo(copyBookmark));
                    continue;
                }

                if (copyBookmark) folder.Add(item.CurrentBookmark.Copy());
            }
            return folder;
        }

        /// <summary>
        /// 恢复名称
        /// </summary>
        public void RecoverName()
        {
            Name = oldName;
        }

        /// <summary>
        /// 更新文件夹信息
        /// </summary>
        public void UpdateFolder()
        {
            SaveFolderItem.Key = CurrentFolder.Id;
            SaveFolderItem.Display = CurrentFolder.Name;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="guidFactory">Id工厂</param>
        /// <param name="parentLevel">父节点等级</param>
        /// <returns></returns>
        public BookmarkTreeViewInfo Copy(Func<Guid> guidFactory, int parentLevel)
        {
            if (IsFolder)
            {
                var copy = CurrentFolder.Copy();
                copy.Id = guidFactory();
                return new BookmarkTreeViewInfo(this, parentLevel, copy, guidFactory);
            }
            else
            {
                var copy = CurrentBookmark.Copy();
                copy.Id = guidFactory();
                return new BookmarkTreeViewInfo(this, parentLevel, copy);
            }
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        /// <param name="infos">粘贴信息</param>
        /// <param name="index">粘贴位置</param>
        public void Paste(List<BookmarkTreeViewInfo> infos, int index)
        {
            if (index == -1)
            {
                index = Children.Count;
            }

            BookmarkTreeViewInfo item;
            for (int i = infos.Count - 1; i >= 0; i--)
            {
                item = infos[i];
                item.Parent = this;
                Children.Insert(index, item);
            }

            RaisePropertyChanged(nameof(HasVisibilityItems));
        }

        public void Delete(BookmarkTreeViewInfo info)
        {
            Children.Remove(info);
            RaisePropertyChanged(nameof(HasVisibilityItems));
        }

        public void Delete(List<BookmarkTreeViewInfo> infos)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var current = children[i];
                var temp = infos.FirstOrDefault(t => t.Id == current.Id);
                if (temp != null)
                {
                    children.RemoveAt(i);
                    infos.Remove(temp);
                    continue;
                }

                if (current.IsFolder)
                {
                    current.Delete(infos);
                }
            }
            RaisePropertyChanged(nameof(HasVisibilityItems));
        }

        public void UpdateFolderName(string name)
        {
            Name = name;
            CurrentFolder.Name = name;
        }

        public void Update(string name, string location)
        {
            Name = name;
            if (IsFolder)
            {
                CurrentFolder.Name = name;
            }
            else
            {
                CurrentBookmark.Name = name;
                CurrentBookmark.Location = location;
            }
        }

        public void Add(BookmarkTreeViewInfo info)
        {
            Children.Add(info);
            RaisePropertyChanged(nameof(HasVisibilityItems));
        }

        private static string GetIcon(bool isBookmark, bool isExpanded, bool isSpecil)
        {
            if (isBookmark) return "📄";
            if (isSpecil) return "⭐";
            return isExpanded ? "📂" : "📁";
        }
    }
}
