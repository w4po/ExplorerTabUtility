using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ExplorerTabUtility.UI.Converters
{
    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            Visibility visValue = parameter == null ? Visibility.Visible : (Visibility)parameter;
            if (boolValue) return visValue;
            return visValue == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
