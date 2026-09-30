using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Languages.Manager;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views.Controls;
using ExplorerTabUtility.WinAPI;
using Microsoft.Win32;

namespace ExplorerTabUtility.UI.Views
{
    /// <summary>
    /// BookmarkManagePopup.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkManagePopup : BaseBookmarkWindow
    {
        private bool needDialogResult;
        private readonly string initBookmarkJson;

        public BookmarkManagePopup(ExplorerWatcher explorerWatcher, nint windowHandle, bool needDialogResult) : base(explorerWatcher, windowHandle)
        {
            InitializeComponent();

            Init(needDialogResult);
            SetupEventHandlers();

            initBookmarkJson = GetJson(TvFolder.CopyFolderInfos());
        }

        public void AddFolder(BookmarkTreeViewInfo info)
        {
            if (TvFolder.AddFolder(info, out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
        }

        #region 私有函数
        private void Init(bool needDialogResult)
        {
            this.needDialogResult = needDialogResult;
            TvFolder.SetItemsSource(BookmarkManager.Bookmarks, BookmarkManager.Folder.Id, true);
        }

        private string GetFileFilter()
        {
            var jsonFiles = LangeuageHelper.Instance.LanguageFields.JsonFiles;
            var allFiles = LangeuageHelper.Instance.LanguageFields.AllFiles;
            return $"{jsonFiles}|*.json|{allFiles}|*.*";
        }

        private void CloseWindow(bool isCancel)
        {
            var dialogResult = false;
            var infos = TvFolder.CopyFolderInfos();
            var afterJson = GetJson(infos);

            if (afterJson != initBookmarkJson)
            {
                if (isCancel)
                {
                    BookmarkManager.RecoverConfig();
                }
                else
                {
                    dialogResult = true;
                    BookmarkManager.Save(infos);
                }
            }

            if (needDialogResult)
            {
                DialogResult = dialogResult;
            }

            LangeuageHelper.Instance.OnLangeuageChanged -= Instance_OnLangeuageChanged;
            CloseWindow();
        }

        private static string GetJson<T>(T t)
        {
            return JsonSerializer.Serialize(t);
        }

        private void DeleteSelectedItems()
        {
            var items = LbChildren.SelectedItems.Cast<BookmarkTreeViewInfo>().ToList();
            if (items.Count > 0)
            {
                TvFolder.Delete(items);

                if (LbChildren.ItemsSource is IEnumerable<BookmarkTreeViewInfo> datas)
                {
                    LbChildren.ItemsSource = datas.Except(items);
                }
            }
        }

        private void Edit(BookmarkTreeViewInfo info)
        {
            var popup = new BookmarkSavePopup(explorerWatcher, windowHandle, info);
            popup.ShowDialog();
        }

        private async void OpenBookmark(BookmarkAction action, bool isFocusedTreeView)
        {
            var selectedInfos = GetOperateInfos(isFocusedTreeView);
            var infos = selectedInfos.SelectMany(t => t.GetBookmarkInfos()).ToList();
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

        private void Cut(bool isFocusedTreeView)
        {
            var cutInfos = GetOperateInfos(isFocusedTreeView);
            BookmarkManager.ClipboardManager.Cut(cutInfos, string.IsNullOrEmpty(TxtSearch.Text));
            TvFolder.Cut(cutInfos);
        }

        private void Copy(bool isFocusedTreeView)
        {
            var copyInfos = GetOperateInfos(isFocusedTreeView);
            BookmarkManager.ClipboardManager.Copy(copyInfos, string.IsNullOrEmpty(TxtSearch.Text));
        }

        private void Paste(bool isFocusedTreeView)
        {
            var parent = (BookmarkTreeViewInfo)TvFolder.SelectedItem;
            var infos = BookmarkManager.ClipboardManager.GetInfos(parent.Level);
            parent.Paste(infos, isFocusedTreeView ? -1 : LbChildren.SelectedIndex + 1);
            parent.IsSelected = true;
        }

        private List<BookmarkTreeViewInfo> GetOperateInfos(bool isFocusedTreeView)
        {
            return isFocusedTreeView
                ? [(BookmarkTreeViewInfo)TvFolder.SelectedItem]
                : LbChildren.SelectedItems.Cast<BookmarkTreeViewInfo>().ToList();
        }

        private void ShowInFolder()
        {
            var showItem = (BookmarkTreeViewInfo)LbChildren.SelectedItem;
            var parent = showItem.Parent;
#pragma warning disable CS8602 // 解引用可能出现空引用。
            parent.IsSelected = true;
            BookmarkTreeView.Expanded(parent, false);
#pragma warning restore CS8602 // 解引用可能出现空引用。
            LbChildren.SelectedItem = showItem;
        }

        private void Import()
        {
            var title = LangeuageHelper.Instance.LanguageFields.ImportBookmark;
            var ofd = new OpenFileDialog
            {
                FileName = Constants.BookmarksFileName,
                Filter = GetFileFilter(),
                Title = title,
            };
            if (ofd.ShowDialog() != true) return;

            var jsonString = System.IO.File.ReadAllText(ofd.FileName, Encoding.UTF8);
            if (BookmarkManager.Import(jsonString))
            {
                TvFolder.SetItemsSource(BookmarkManager.Bookmarks, BookmarkManager.Folder.Id, true);
                ShowMessage(LangeuageHelper.Instance.LanguageFields.ImportSuccessful, Constants.AppName);
            }
            else
            {
                ShowMessage(LangeuageHelper.Instance.LanguageFields.ImportFailed, Constants.AppName, icon: MessageBoxImage.Error);
            }
        }

        private void Export(bool save)
        {
            var title = save ? LangeuageHelper.Instance.LanguageFields.SaveAndExportBookmark : LangeuageHelper.Instance.LanguageFields.ExportBookmark;
            var fileName = Constants.BookmarksFileName.Insert(Constants.BookmarksFileName.IndexOf("."), DateTime.Now.ToString("yyyy-MM-dd"));
            var sfd = new SaveFileDialog
            {
                FileName = fileName,
                Filter = GetFileFilter(),
                Title = title,
            };
            if (sfd.ShowDialog() != true) return;

            if (save)
            {
                var infos = TvFolder.CopyFolderInfos();
                var afterJson = GetJson(infos);

                if (afterJson != initBookmarkJson)
                {
                    BookmarkManager.Save(infos);
                }
            }

            using var openFile = sfd.OpenFile();
            var jsonString = BookmarkManager.Export();
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            openFile.Write(bytes, 0, bytes.Length);
        }
        #endregion

        #region 事件注册
        private void SetupEventHandlers()
        {
            KeyDown += BookmarkManagePopup_KeyDown;
            BtnCancel.Click += BtnCancel_Click;
            BtnSave.Click += BtnSave_Click;
            BtnNewFolder.Click += BtnNewFolder_Click;
            TvFolder.SelectedItemChanged += TvFolder_SelectedItemChanged;
            TvFolder.BookmarkHandle += TvFolder_BookmarkHandle;
            TvFolder.FolderHandle += TvFolder_FolderHandle;
            LbChildren.KeyDown += LbChildren_KeyDown;
            TxtSearch.GotFocus += TxtSearch_GotFocus;
            TxtSearch.TextChanged += TxtSearch_TextChanged;
            TxtSearch.KeyDown += TxtSearch_KeyDown;
            MenuImport.Click += MenuImport_Click;
            MenuExport.Click += MenuExport_Click;
            MenuSaveAndExport.Click += MenuSaveAndExport_Click;
            LangeuageHelper.Instance.OnLangeuageChanged += Instance_OnLangeuageChanged;
        }

        private void Instance_OnLangeuageChanged()
        {
            TvFolder.OnLangeuageChanged();
        }

        private void MenuSaveAndExport_Click(object sender, RoutedEventArgs e)
        {
            Export(true);
        }

        private void MenuExport_Click(object sender, RoutedEventArgs e)
        {
            Export(false);
        }

        private void MenuImport_Click(object sender, RoutedEventArgs e)
        {
            Import();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && LbChildren.ItemsSource.GetEnumerator().MoveNext())
            {
                LbChildren.Focus();
                LbChildren.SelectedIndex = 0;
            }
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtSearch.SelectAll();
        }

        private void LbChildren_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteSelectedItems();
                    e.Handled = true;
                    break;
                case Key.F2:
                    if (LbChildren.SelectedItems.Count == 1)
                    {
                        var info = (BookmarkTreeViewInfo)LbChildren.SelectedItem;
                        Edit(info);
                        e.Handled = true;
                    }
                    break;
                case Key.X:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        Cut(false);
                    }
                    break;
                case Key.C:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        Copy(false);
                    }
                    break;
                case Key.V:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control) && BookmarkManager.ClipboardManager.ListBoxCanPaste)
                    {
                        Paste(false);
                    }
                    break;
                case Key.G:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control) && BookmarkManager.ClipboardManager.IsSearch && LbChildren.SelectedItems.Count == 1)
                    {
                        ShowInFolder();
                    }
                    break;
                case Key.O:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control) && LbChildren.SelectedItems.Count == 1)
                    {
                        var info = (BookmarkTreeViewInfo)LbChildren.SelectedItem;
                        if (info.IsFolder == false)
                        {
                            TvFolder_BookmarkHandle(info, info.CurrentBookmark, BookmarkAction.OpenInCurrentTab);
                        }
                    }
                    break;
                case Key.T:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        OpenBookmark(BookmarkAction.OpenInNewTab, false);
                    }
                    break;
                case Key.W:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        OpenBookmark(BookmarkAction.OpenInNewWindow, false);
                    }
                    break;
            }
        }

        private void TvFolder_FolderHandle(BookmarkTreeViewInfo info, FolderInfo folder, BookmarkAction action)
        {
            var isFocusedTreeView = FocusManager.GetFocusedElement(this) is TreeViewItem;
            switch (action)
            {
                case BookmarkAction.OpenInNewTab:
                case BookmarkAction.OpenInNewWindow:
                    OpenBookmark(action, isFocusedTreeView);
                    break;
                case BookmarkAction.Delete:
                    if (isFocusedTreeView)
                    {
                        TvFolder.Delete();
                    }
                    else
                    {
                        DeleteSelectedItems();
                    }
                    break;
                case BookmarkAction.Rename:
                    if (isFocusedTreeView)
                    {
                        TvFolder.Rename();
                    }
                    else
                    {
                        Edit(info);
                    }
                    break;
                case BookmarkAction.Cut:
                    Cut(isFocusedTreeView);
                    break;
                case BookmarkAction.Copy:
                    Copy(isFocusedTreeView);
                    break;
                case BookmarkAction.Paste:
                    Paste(isFocusedTreeView);
                    break;
                case BookmarkAction.ShowInFolder:
                    ShowInFolder();
                    break;
            }
        }

        private async void TvFolder_BookmarkHandle(BookmarkTreeViewInfo info, BookmarkInfo bookmark, BookmarkAction action)
        {
            switch (action)
            {
                case BookmarkAction.OpenInCurrentTab:
                    await explorerWatcher.Open(bookmark.Location, true, windowHandle, inCurrentTab: true);
                    break;
                case BookmarkAction.OpenInNewTab:
                case BookmarkAction.OpenInNewWindow:
                    OpenBookmark(action, false);
                    break;
                case BookmarkAction.Delete:
                    DeleteSelectedItems();
                    break;
                case BookmarkAction.Edit:
                    Edit(info);
                    break;
                case BookmarkAction.Cut:
                    Cut(false);
                    break;
                case BookmarkAction.Copy:
                    Copy(false);
                    break;
                case BookmarkAction.Paste:
                    Paste(false);
                    break;
                case BookmarkAction.ShowInFolder:
                    ShowInFolder();
                    break;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var key = TxtSearch.Text;
            if (string.IsNullOrEmpty(key))
            {
                BookmarkManager.ClipboardManager.SetListBoxCanPaste(true);
                var info = (BookmarkTreeViewInfo)TvFolder.SelectedItem;
                if (info != null) LbChildren.ItemsSource = info.Children;
            }
            else
            {
                BookmarkManager.ClipboardManager.SetListBoxCanPaste(false);
                LbChildren.ItemsSource = TvFolder.Search(key);
            }
        }

        private void TvFolder_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (string.IsNullOrEmpty(TxtSearch.Text))
            {
                var info = (BookmarkTreeViewInfo)e.NewValue;
                if (info != null) LbChildren.ItemsSource = info.Children;
            }
            else
            {
                TxtSearch.Text = string.Empty;
            }
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            if (TvFolder.AddFolder(out var errorMsg) == false)
            {
                ShowMessage(errorMsg, Constants.AppName);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(false);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(true);
        }

        private void BookmarkManagePopup_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    CloseWindow(true);
                    break;
                case Key.F:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        TxtSearch.Focus();
                    }
                    break;
                case Key.S:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        CloseWindow(false);
                    }
                    break;
                case Key.I:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        Import();
                    }
                    break;
                case Key.E:
                    if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Control))
                    {
                        if (KeyboardSimulator.IsKeyPressed((int)VirtualKey.Shift))
                        {
                            Export(true);
                        }
                        else
                        {
                            Export(false);
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}
