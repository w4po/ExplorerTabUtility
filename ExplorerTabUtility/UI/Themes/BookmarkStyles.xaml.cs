using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;
using ExplorerTabUtility.UI.Views;

namespace ExplorerTabUtility.UI.Themes
{
    public partial class BookmarkStyles
    {
        private bool isCancel;

        private void BookmarkEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            var txt = (TextBox)sender;
            var info = (BookmarkTreeViewInfo)txt.DataContext;

            if (isCancel || string.IsNullOrEmpty(info.Name))
            {
                info.RecoverName();
                if (info.IsAdd)
                {
                    AddFolder(txt, info);
                }
            }
            else if (info.IsNeedSave)
            {
                AddFolder(txt, info);
            }

            isCancel = false;
        }

        private void BookmarkEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                isCancel = e.Key == Key.Escape;
                if (isCancel) e.Handled = true;

                var txt = (TextBox)sender;
                var item = VisualTreeHelperEx.GetParent<TreeViewItem>(txt);
                if (item != null)
                {
                    item.Focus();
                }
            }
        }

        private void AddFolder(TextBox txt, BookmarkTreeViewInfo info)
        {
            var popup = VisualTreeHelperEx.GetParent<BookmarkSavePopup>(txt);
            if (popup != null)
            {
                popup.AddFolder(info);
                return;
            }

            var managePopup = VisualTreeHelperEx.GetParent<BookmarkManagePopup>(txt);
            if (managePopup != null)
            {
                managePopup.AddFolder(info);
            }
        }
    }
}
