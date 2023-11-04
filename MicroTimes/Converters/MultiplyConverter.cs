using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MicroTimes.Converters;

public class MultiplyConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double numericValue && parameter is double numericParameter)
        {
            return numericValue * numericParameter;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double numericValue && parameter is double numericParameter)
        {
            return numericValue / numericParameter;
        }

        return value;
    }
}