using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ExplorerTabUtility.UI.Converters
{
    class LevelToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (int)value;
            var unit = (double)parameter;
            return new Thickness(level * unit, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
