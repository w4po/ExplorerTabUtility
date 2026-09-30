using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.UI.Behaviors
{
    internal class BookmarkHelper
    {
        #region 选择其他文件夹文本
        public static string GetSelectOtherFolderText(DependencyObject obj)
        {
            return (string)obj.GetValue(SelectOtherFolderTextProperty);
        }

        public static void SetSelectOtherFolderText(DependencyObject obj, string value)
        {
            obj.SetValue(SelectOtherFolderTextProperty, value);
        }

        public static readonly DependencyProperty SelectOtherFolderTextProperty =
            DependencyProperty.RegisterAttached("SelectOtherFolderText", typeof(string), typeof(BookmarkHelper), new PropertyMetadata(""));
        #endregion

        #region MVVM聚焦
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool?), typeof(BookmarkHelper), new FrameworkPropertyMetadata(IsFocusedChanged) { BindsTwoWayByDefault = true });

        public static bool? GetIsFocused(DependencyObject element)
        {
            return (bool?)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool? value)
        {
            element.SetValue(IsFocusedProperty, value);
        }

        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;

            if (e.OldValue == null)
            {
                fe.GotFocus += FrameworkElement_GotFocus;
                fe.LostFocus += FrameworkElement_LostFocus;
            }

            if (!fe.IsVisible)
            {
                fe.IsVisibleChanged += new DependencyPropertyChangedEventHandler(FrameworkElement_IsVisibleChanged);
            }

            if ((bool)e.NewValue)
            {
                if (fe is TextBox txt) txt.SelectAll();
                fe.Focus();
            }
        }

        private static void FrameworkElement_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            if (fe.IsVisible && (bool)((FrameworkElement)sender).GetValue(IsFocusedProperty))
            {
                fe.IsVisibleChanged -= FrameworkElement_IsVisibleChanged;
                fe.Focus();
            }
        }

        private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        }

        private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }
        #endregion

        #region 聚焦到选中行
        public const double TreeViewItemLeftOffset = 20;

        public static readonly DependencyProperty IsBringIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
                "IsBringIntoViewWhenSelected",
                typeof(bool),
                typeof(BookmarkHelper),
                new PropertyMetadata(false, OnIsBringIntoViewWhenSelectedChanged));

        public static bool GetIsBringIntoViewWhenSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsBringIntoViewWhenSelectedProperty);
        }

        public static void SetIsBringIntoViewWhenSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsBringIntoViewWhenSelectedProperty, value);
        }

        private static void OnIsBringIntoViewWhenSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeViewItem item)
            {
                if ((bool)e.NewValue)
                {
                    item.Selected += Item_Selected;
                }
                else
                {
                    item.Selected -= Item_Selected;
                }
            }
        }

        private static TreeViewItem? lastBringItem;

        private static void Item_Selected(object sender, RoutedEventArgs e)
        {
            // 确保只有当前选中的项触发，避免冒泡导致的问题
            if (e.OriginalSource is TreeViewItem selectedItem)
            {
                if (lastBringItem == selectedItem) return;

                lastBringItem = selectedItem;
                selectedItem.BringIntoView();

                var scrollViewer = VisualTreeHelperEx.GetParent<ScrollViewer>(selectedItem);
                if (scrollViewer != null)
                {
                    var info = (BookmarkTreeViewInfo)selectedItem.DataContext;
                    var left = info.Level * TreeViewItemLeftOffset;

                    var svWidth = scrollViewer.ActualWidth;
                    var offset = scrollViewer.HorizontalOffset;

                    var diff = left - offset;
                    if (diff > 0 && diff < svWidth / 2)
                    {
                        diff = 0;
                    }

                    scrollViewer.ScrollToHorizontalOffset(offset + diff);
                }
            }
        }
        #endregion

        #region 左键点击时打开菜单
        private static bool cancelOpen;

        public static bool GetOpenContentMenuOnLeftClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(OpenContentMenuOnLeftClickProperty);
        }

        public static void SetOpenContentMenuOnLeftClick(DependencyObject obj, bool value)
        {
            obj.SetValue(OpenContentMenuOnLeftClickProperty, value);
        }

        public static readonly DependencyProperty OpenContentMenuOnLeftClickProperty =
            DependencyProperty.RegisterAttached("OpenContentMenuOnLeftClick", typeof(bool), typeof(BookmarkHelper), new PropertyMetadata(false, OnOpenContentMenuOnLeftClickChanged));

        private static void OnOpenContentMenuOnLeftClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
                    element.PreviewMouseRightButtonDown += Element_PreviewMouseRightButtonDown;
                    element.ContextMenuOpening += Element_ContextMenuOpening;
                }
                else
                {
                    element.PreviewMouseLeftButtonDown -= Element_PreviewMouseLeftButtonDown;
                    element.PreviewMouseRightButtonDown -= Element_PreviewMouseRightButtonDown;
                    element.ContextMenuOpening -= Element_ContextMenuOpening;
                }
            }
        }

        private static void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.ContextMenu != null)
            {
                cancelOpen = false;
                element.ContextMenu.PlacementTarget = element;
                element.ContextMenu.IsOpen = true;
            }
        }

        private static void Element_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            cancelOpen = true;
        }

        private static void Element_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (cancelOpen)
            {
                e.Handled = true;
            }
        }
        #endregion
    }
}
