using System;
using System.Windows;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Languages.Manager;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;
using ExplorerTabUtility.WinAPI;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkSavePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkSavePopup : BaseBookmarkWindow
    {
        private BookmarkSaveType saveType;
        private readonly bool? isEdit;
        private readonly Guid parentId;
        private readonly BookmarkInfo currentBookmarkInfo;
        private readonly FolderInfo currentFolderInfo;
        private readonly BookmarkTreeViewInfo? bookmarkTreeViewInfo;

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            currentBookmarkInfo = CreateCurrentLocationBookmark();
            currentFolderInfo = FolderInfo.Empty;
            saveType = BookmarkSaveType.ComboBox;

            Init();
            SetupEventHandlers();
        }

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle, FolderInfo folderInfo, Guid parentId) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            this.parentId = parentId;
            TxtTitle.Text = folderInfo.Id == Guid.Empty ? LangeuageHelper.Instance.LanguageFields.NewFolder : LangeuageHelper.Instance.LanguageFields.Rename;
            currentBookmarkInfo = BookmarkInfo.Empty;
            currentFolderInfo = folderInfo;
            saveType = BookmarkSaveType.FolderRename;

            Init();
            SetupBaseEventHandlers();
        }

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle, BookmarkInfo bookmarkInfo, Guid parentId) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            this.parentId = parentId;
            isEdit = bookmarkInfo.Id != Guid.Empty;
            TxtTitle.Text = isEdit.Value ? LangeuageHelper.Instance.LanguageFields.Edit : LangeuageHelper.Instance.LanguageFields.AddBookmark;
            currentBookmarkInfo = bookmarkInfo;
            currentFolderInfo = FolderInfo.Empty;
            saveType = BookmarkSaveType.TreeView;

            Init();
            SetupEventHandlers();
        }

        public BookmarkSavePopup(ExplorerWatcher explorerWatcher, nint windowHandle, BookmarkTreeViewInfo info) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            bookmarkTreeViewInfo = info;
            TxtName.Text = info.Name;
            if (info.IsFolder)
            {
                currentBookmarkInfo = BookmarkInfo.Empty;
                currentFolderInfo = info.CurrentFolder;


                SpLocation.Visibility = Visibility.Collapsed;
                TxtTitle.Text = LangeuageHelper.Instance.LanguageFields.Rename;
            }
            else
            {
                currentBookmarkInfo = info.CurrentBookmark;
                currentFolderInfo = FolderInfo.Empty;

                TxtLocation.Text = currentBookmarkInfo.Location;
                TxtTitle.Text = LangeuageHelper.Instance.LanguageFields.Edit;
            }

            BtnNewFolder.Visibility = Visibility.Collapsed;
            TxtFolder.Visibility = Visibility.Collapsed;
            CbSelectSavePath.Visibility = Visibility.Collapsed;
            TvSelectSavePath.Visibility = Visibility.Collapsed;
            TxtName.Focus();
            TxtName.SelectionStart = TxtName.Text.Length;

            SetupBaseEventHandlers();
        }

        public void AddFolder(BookmarkTreeViewInfo info)
        {
            if (TvSelectSavePath.AddFolder(info, out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
        }

        private void Init()
        {
            switch (saveType)
            {
                case BookmarkSaveType.FolderRename:
                    TxtName.Text = currentFolderInfo.Name;
                    SpLocation.Visibility = Visibility.Collapsed;
                    BtnNewFolder.Visibility = Visibility.Collapsed;
                    TxtFolder.Visibility = Visibility.Collapsed;
                    CbSelectSavePath.Visibility = Visibility.Collapsed;
                    TvSelectSavePath.Visibility = Visibility.Collapsed;
                    break;
                case BookmarkSaveType.ComboBox:
                    BtnNewFolder.IsEnabled = false;
                    SpLocation.Visibility = Visibility.Collapsed;
                    BtnNewFolder.Visibility = Visibility.Collapsed;
                    TvSelectSavePath.Visibility = Visibility.Collapsed;

                    TxtName.Text = currentBookmarkInfo.Name;

                    InitCbSelectSavePath();
                    break;
                case BookmarkSaveType.TreeView:
                    BtnNewFolder.IsEnabled = false;
                    CbSelectSavePath.Visibility = Visibility.Collapsed;

                    TxtName.Text = currentBookmarkInfo.Name;
                    TxtLocation.Text = currentBookmarkInfo.Location;
                    TxtLocation.ToolTip = currentBookmarkInfo.Location;

                    InitTvSelectSavePath(parentId);
                    break;
            }
            TxtName.Focus();
            TxtName.SelectionStart = TxtName.Text.Length;
        }

        private void InitCbSelectSavePath()
        {
            CbSelectSavePath.SetItemsSource(BookmarkManager.LastSaveFolders, BookmarkManager.LastSaveFolderId);
        }

        private void InitTvSelectSavePath(Guid parentId)
        {
            TvSelectSavePath.SetItemsSource(BookmarkManager.Bookmarks, parentId, false);
        }

        private SaveFolderItem? GetSaveFolder()
        {
            return saveType switch
            {
                BookmarkSaveType.ComboBox => (SaveFolderItem)CbSelectSavePath.SelectedItem,
                BookmarkSaveType.TreeView => TvSelectSavePath.GetSaveFolderItem(),
                _ => null,
            };
        }

        private string GetSaveLocation()
        {
            return saveType switch
            {
                BookmarkSaveType.ComboBox => currentBookmarkInfo.Location,
                _ => TxtLocation.Text,
            };
        }

        private void CloseWindow(bool isCancel)
        {
            if (isCancel && TvSelectSavePath.HaveSave)
            {
                BookmarkManager.RecoverConfig();
            }

            LangeuageHelper.Instance.OnLangeuageChanged -= Instance_OnLangeuageChanged;
            CloseWindow();
        }

        private void Save()
        {
            if (bookmarkTreeViewInfo != null)
            {
                bookmarkTreeViewInfo.Update(TxtName.Text, TxtLocation.Text);
                if (bookmarkTreeViewInfo.IsFolder)
                {
                    BookmarkManager.Save(parentId, currentFolderInfo, TxtName.Text, false);
                }
                DialogResult = true;
            }
            else if (saveType == BookmarkSaveType.FolderRename)
            {
                if (BookmarkManager.Save(parentId, currentFolderInfo, TxtName.Text, true) == false)
                {
                    ShowMessage(LangeuageHelper.Instance.LanguageFields.SaveFailed, Constants.AppName);
                    return;
                }

                DialogResult = true;
            }
            else
            {
                var saveFolder = GetSaveFolder();
                if (saveFolder == null)
                {
                    ShowMessage(LangeuageHelper.Instance.LanguageFields.PleaseSelectSaveLocation, Constants.AppName);
                    return;
                }

                if (BookmarkManager.Save(isEdit ?? false, parentId, saveFolder.Key, currentBookmarkInfo, TxtName.Text, GetSaveLocation()) == false)
                {
                    ShowMessage(LangeuageHelper.Instance.LanguageFields.SaveFailed, Constants.AppName);
                    return;
                }

                currentBookmarkInfo.ParentId = saveFolder.Key;
                if (isEdit.HasValue) DialogResult = true;
            }

            CloseWindow(false);
        }

        #region 事件注册
        private void SetupBaseEventHandlers()
        {
            KeyDown += BookmarkSavePopup_KeyDown;
            BtnSave.Click += BtnSave_Click;
            BtnCancel.Click += BtnCancel_Click;
            TvSelectSavePath.FolderHandle += TvSelectSavePath_FolderHandle;
            LangeuageHelper.Instance.OnLangeuageChanged += Instance_OnLangeuageChanged;
        }

        private void SetupEventHandlers()
        {
            SetupBaseEventHandlers();

            SizeChanged += BookmarkSavePopup_SizeChanged;
            BtnNewFolder.Click += BtnNewFolder_Click;
            TvSelectSavePath.SelectedItemChanged += TvSelectSavePath_SelectedItemChanged;

            if (saveType == BookmarkSaveType.ComboBox)
            {
                CbSelectSavePath.SelectOtherFolderClick += CbSelectSavePath_SelectOtherFolderClick;
            }
        }

        private void Instance_OnLangeuageChanged()
        {
            CbSelectSavePath.OnLangeuageChanged();
            TvSelectSavePath.OnLangeuageChanged();
        }

        private void TvSelectSavePath_FolderHandle(BookmarkTreeViewInfo info, FolderInfo folder, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.Rename:
                    TvSelectSavePath.Rename();
                    break;
                case BookmarkAction.Delete:
                    TvSelectSavePath.Delete();
                    break;
            }
        }

        private void TvSelectSavePath_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BtnNewFolder.IsEnabled = TvSelectSavePath.SelectedItem != null;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(true);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            if (TvSelectSavePath.AddFolder(out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
        }

        private void CbSelectSavePath_SelectOtherFolderClick(object sender, RoutedEventArgs e)
        {
            SpLocation.Visibility = Visibility.Visible;
            BtnNewFolder.Visibility = Visibility.Visible;
            TvSelectSavePath.Visibility = Visibility.Visible;
            CbSelectSavePath.Visibility = Visibility.Collapsed;

            saveType = BookmarkSaveType.TreeView;
            TxtLocation.Text = currentBookmarkInfo.Location;
            TxtLocation.ToolTip = currentBookmarkInfo.Location;

            var parentId = BookmarkManager.LastSaveFolderId == Guid.Empty
                ? BookmarkManager.LastSaveFolders[0].Id
                : BookmarkManager.LastSaveFolderId;
            InitTvSelectSavePath(parentId);
        }

        private void BookmarkSavePopup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height != 0) Top -= (e.NewSize.Height - e.PreviousSize.Height) / 2;
        }

        private void BookmarkSavePopup_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    CloseWindow(true);
                    break;
                case Key.S:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        Save();
                    }
                    break;
            }
        }
        #endregion
    }
}
