using System;
using System.IO;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ExplorerTabUtility.UI.Converters;

public class Base64ImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string source)
            return null;

        // Check if it's a data URL
        if (source.StartsWith("data:image"))
        {
            try
            {
                // Extract the base64 part
                var commaIndex = source.IndexOf(',');
                if (commaIndex > 0)
                {
                    var base64Data = source.Substring(commaIndex + 1);
                    var imageBytes = System.Convert.FromBase64String(base64Data);

                    using var ms = new MemoryStream(imageBytes);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting base64 image: {ex.Message}");
            }
        }

        // If it's not a data URL or conversion failed, try to load it as a regular URL or file path
        try
        {
            return new BitmapImage(new Uri(source, UriKind.RelativeOrAbsolute));
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}