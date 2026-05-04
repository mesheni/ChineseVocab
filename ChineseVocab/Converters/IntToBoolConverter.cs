using System.Globalization;

namespace ChineseVocab.Converters;

/// <summary>
/// Конвертирует int > 0 в true. Используется для видимости HSK бейджа.
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int intValue && intValue > 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
