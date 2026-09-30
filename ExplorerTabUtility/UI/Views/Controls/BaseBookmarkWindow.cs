using System;
using System.IO;
using System.Windows;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Hooks;
using ExplorerTabUtility.Languages.Manager;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.UI.Views.Controls
{
    public class BaseBookmarkWindow(ExplorerWatcher explorerWatcher, nint windowHandle) : Window
    {
        protected readonly ExplorerWatcher explorerWatcher = explorerWatcher;
        protected readonly nint windowHandle = windowHandle;
        private bool _isShowingDialog;
        private bool _isClosing;

        public new void Show()
        {
            base.Show();
            if (Activate()) return;

            Helper.BypassWinForegroundRestrictions();
            Activate();
        }

        /// <summary>
        /// 创建当前标签页书签
        /// </summary>
        /// <returns></returns>
        protected BookmarkInfo CreateCurrentLocationBookmark()
        {
            var record = explorerWatcher.GetCurrentTabWindowRecord(windowHandle);
            var location = record == null ? string.Empty : record.DisplayLocation;
            return new BookmarkInfo(Guid.Empty, GetName(location), location);
        }

        /// <summary>
        /// 根据路径获取名称
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        protected static string GetName(string location)
        {
            return location switch
            {
                "shell:::{20D04FE0-3AEA-1069-A2D8-08002B30309D}" => LangeuageHelper.Instance.LanguageFields.ThisPc,
                "shell:::{645FF040-5081-101B-9F08-00AA002F954E}" => LangeuageHelper.Instance.LanguageFields.RecycleBin,
#if NET481
                _ => location.EndsWith(":") ? string.Format(LangeuageHelper.Instance.LanguageFields.DriveFormat, location.TrimEnd(':')) : Path.GetFileName(location),
#elif NET9_0
                _ => location.EndsWith(':') ? string.Format(LangeuageHelper.Instance.LanguageFields.DriveFormat, location.TrimEnd(':')) : Path.GetFileName(location),
#endif
            };
        }

        protected bool CanClose()
        {
            return _isShowingDialog == false;
        }

        protected void CloseWindow()
        {
            if (_isClosing) return;
            _isClosing = true;

            Dispatcher.BeginInvoke(() =>
            {
                Close();
            });
        }

        protected MessageBoxResult ShowMessage(string message, string title,
            MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultButton = MessageBoxResult.None)
        {
            _isShowingDialog = true;
            var result = CustomMessageBox.Show(message, title, buttons, icon, defaultButton);
            _isShowingDialog = false;
            return result;
        }

        protected void EntryDialog()
        {
            _isShowingDialog = true;
        }

        protected void ExitDialog()
        {
            _isShowingDialog = false;
        }
    }
}
