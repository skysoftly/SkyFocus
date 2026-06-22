using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace SkyFocus.Converter;

public class PathToFolderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            try
            {
                // Получаем папку
                string? directory = Path.GetDirectoryName(path);
                return directory ?? string.Empty;
            }
            catch
            {
                // Если путь некорректный
                return string.Empty;
            }
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не нужно
        throw new NotImplementedException();
    }
}