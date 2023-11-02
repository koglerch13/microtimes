using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MicroTimes.Converters;

public class TimeOnlyStringConverter : IValueConverter
{
    private const string FORMAT = "HH:mm";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var time = value as TimeOnly?;

        if (time == null)
            return string.Empty;

        return time.Value.ToString(FORMAT);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var timeString = value as string;
        if (string.IsNullOrEmpty(timeString))
            return null;

        var canParse = TimeOnly.TryParseExact(timeString, FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time);
        if (!canParse)
            return null;

        return time;
    }
}