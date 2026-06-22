using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SkyFocus.Converters;

public class SecondsToTimeStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int seconds = 0;
        
        if (value is int intVal)
            seconds = intVal;
        else if (value is long longVal)
            seconds = (int)longVal;
        else if (value is double doubleVal)
            seconds = (int)doubleVal;
        else if (value is string strVal && int.TryParse(strVal, out int parsed))
            seconds = parsed;
        else
            return "0м";

        var ts = TimeSpan.FromSeconds(seconds);

        var hours = (int)ts.TotalHours;
        var minutes = ts.Minutes;

        if (hours > 0)
            return $"{hours}ч {minutes}м";

        if (minutes > 0)
            return $"{minutes}м";
            
        return "0м";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}