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

        var ts = TimeSpan.FromSeconds(seconds);

        var days = (int)ts.TotalDays;
        var hours = ts.Hours;
        var minutes = ts.Minutes;

        if (days > 0)
            return $"{days}д {hours}ч";

        if (hours > 0)
            return $"{hours}ч {minutes}м";

        if (minutes > 0)
            return $"{minutes}м";
            
        return "0м";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}