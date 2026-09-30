using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.UI.Converters
{
    internal class ComboBoxItemInfoConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return DependencyProperty.UnsetValue;

            string? tooltip = null;
            if (value is ComboBoxItemInfo<HotKeyAction> actionInfo)
            {
                tooltip = actionInfo.ToolTip;
            }
            else if (value is ComboBoxItemInfo<HotkeyScope> scopeInfo)
            {
                tooltip = scopeInfo.ToolTip;
            }
            else if (value is ComboBoxItemInfo info)
            {
                tooltip = info.ToolTip;
            }

            return tooltip ?? DependencyProperty.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
