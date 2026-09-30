using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerTabUtility.Helpers;

namespace ExplorerTabUtility.UI.Behaviors
{
    class TreeViewHelper
    {
        #region 是否有额外的菜单项
        public static bool GetHaveExtraMenu(DependencyObject obj)
        {
            return (bool)obj.GetValue(HaveExtraMenuProperty);
        }

        public static void SetHaveExtraMenu(DependencyObject obj, bool value)
        {
            obj.SetValue(HaveExtraMenuProperty, value);
        }

        public static readonly DependencyProperty HaveExtraMenuProperty =
            DependencyProperty.RegisterAttached("HaveExtraMenu", typeof(bool), typeof(TreeViewHelper), new PropertyMetadata(false));
        #endregion

        #region 右键点击自动选中
        public static bool GetEnableRightClickSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableRightClickSelectionProperty);
        }

        public static void SetEnableRightClickSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableRightClickSelectionProperty, value);
        }

        public static readonly DependencyProperty EnableRightClickSelectionProperty =
            DependencyProperty.RegisterAttached("EnableRightClickSelection", typeof(bool), typeof(TreeViewHelper), new PropertyMetadata(false, OnEnableRightClickSelectionChanged));

        private static void OnEnableRightClickSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeView treeView)
            {
                if ((bool)e.NewValue)
                {
                    treeView.PreviewMouseRightButtonDown += TreeView_PreviewMouseRightButtonDown;
                }
                else
                {
                    treeView.PreviewMouseRightButtonDown -= TreeView_PreviewMouseRightButtonDown;
                }
            }
        }

        private static void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var obj = e.OriginalSource as DependencyObject;
            if (obj == null) return;

            var item = VisualTreeHelperEx.GetParent<TreeViewItem>(obj);
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
                e.Handled = true;
            }
        }
        #endregion
    }
}
