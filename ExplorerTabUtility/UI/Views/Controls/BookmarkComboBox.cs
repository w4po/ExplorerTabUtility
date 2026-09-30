using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExplorerTabUtility.Languages.Manager;
using ExplorerTabUtility.Managers;
using ExplorerTabUtility.Models;
using SaveFolderItem = ExplorerTabUtility.Models.ComboBoxItemInfo<System.Guid>;

namespace ExplorerTabUtility.UI.Views.Controls
{
    internal class BookmarkComboBox : ComboBox
    {
        #region 自定义事件
        public static readonly RoutedEvent SelectOtherFolderClickEvent =
            EventManager.RegisterRoutedEvent(
                "SelectOtherFolderClick",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(BookmarkComboBox));

        public event RoutedEventHandler SelectOtherFolderClick
        {
            add { AddHandler(SelectOtherFolderClickEvent, value); }
            remove { RemoveHandler(SelectOtherFolderClickEvent, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var btn = GetTemplateChild("BtnSelectOtherFolder") as Button;
            if (btn != null)
            {
                btn.Click += (sender, e) =>
                {
                    RoutedEventArgs args = new RoutedEventArgs(SelectOtherFolderClickEvent);
                    RaiseEvent(args);
                };
            }
        }

        public void SetItemsSource(IReadOnlyList<SaveFolderInfo> datas, Guid selectedId)
        {
            var lastSaveFolders = datas.Select(t => new SaveFolderItem(t.Id, t.Name)).ToList();
            ItemsSource = lastSaveFolders;
            SelectedItem = lastSaveFolders.FirstOrDefault(t => t.Key == selectedId) ?? lastSaveFolders.First();
        }

        public void OnLangeuageChanged()
        {
            var datas = (List<SaveFolderItem>)ItemsSource;
            if (datas == null) return;

            foreach (var item in datas)
            {
                if (item.Key == BookmarkManager.Folder.Id)
                {
                    item.Display = LangeuageHelper.Instance.LanguageFields.BookmarkBar;
                }
                else if (item.Key == BookmarkManager.OtherFolder.Id)
                {
                    item.Display = LangeuageHelper.Instance.LanguageFields.OtherBookmark;
                }
            }

            var index = SelectedIndex;
            SelectedIndex = -1;
            SelectedIndex = index;
        }
    }
}
