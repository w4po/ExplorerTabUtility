using System.Windows;
using System.Windows.Controls;

namespace ExplorerTabUtility.UI.Behaviors
{
    internal class ListBoxHelper
    {
        #region 是否多选
        public static bool GetIsMultipleSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMultipleSelectedProperty);
        }

        public static void SetIsMultipleSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMultipleSelectedProperty, value);
        }

        public static readonly DependencyProperty IsMultipleSelectedProperty =
            DependencyProperty.RegisterAttached("IsMultipleSelected", typeof(bool), typeof(ListBoxHelper), new PropertyMetadata(false));
        #endregion

        #region 监控多选
        public static bool GetMonitorSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(MonitorSelectionProperty);
        }

        public static void SetMonitorSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(MonitorSelectionProperty, value);
        }

        public static readonly DependencyProperty MonitorSelectionProperty =
            DependencyProperty.RegisterAttached("MonitorSelection", typeof(bool), typeof(ListBoxHelper), new PropertyMetadata(false, OnMonitorSelectionChanged));

        private static void OnMonitorSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox lb)
            {
                if ((bool)e.NewValue)
                {
                    lb.SelectionChanged += ListBox_SelectionChanged;

                    var isMulti = lb.SelectedItems.Count > 1;
                    SetIsMultipleSelected(lb, isMulti);
                }
                else
                {
                    lb.SelectionChanged -= ListBox_SelectionChanged;
                }
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListBox)sender;
            var isMulti = lb.SelectedItems.Count > 1;
            SetIsMultipleSelected(lb, isMulti);
        }
        #endregion
    }
}
