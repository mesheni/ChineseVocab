using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ChineseVocab.Converters
{
    /// <summary>
    /// Конвертер для преобразования double в int (с округлением).
    /// Используется для отображения скорости анимации (например, 1x, 2x).
    /// </summary>
    public class DoubleToIntConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует double значение в int с округлением.
        /// </summary>
        /// <param name="value">Double значение для преобразования.</param>
        /// <param name="targetType">Тип целевого свойства (ожидается int или string).</param>
        /// <param name="parameter">Дополнительный параметр (не используется).</param>
        /// <param name="culture">Культура для форматирования.</param>
        /// <returns>Целое число, полученное округлением double значения.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return (int)Math.Round(doubleValue);
            }
            else if (value is float floatValue)
            {
                return (int)Math.Round(floatValue);
            }
            else if (value is decimal decimalValue)
            {
                return (int)Math.Round(decimalValue);
            }
            else if (value is int intValue)
            {
                return intValue; // Уже целое число
            }
            else if (value is null)
            {
                return 0;
            }

            // Если значение другого типа, пытаемся преобразовать
            try
            {
                double convertedValue = System.Convert.ToDouble(value);
                return (int)Math.Round(convertedValue);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Преобразует int значение обратно в double.
        /// </summary>
        /// <param name="value">Int значение для обратного преобразования.</param>
        /// <param name="targetType">Тип целевого свойства (ожидается double).</param>
        /// <param name="parameter">Дополнительный параметр (не используется).</param>
        /// <param name="culture">Культура для форматирования.</param>
        /// <returns>Double значение.</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return (double)intValue;
            }
            else if (value is double doubleValue)
            {
                return doubleValue;
            }
            else if (value is float floatValue)
            {
                return (double)floatValue;
            }
            else if (value is decimal decimalValue)
            {
                return (double)decimalValue;
            }
            else if (value is string stringValue && int.TryParse(stringValue, out int parsedValue))
            {
                return (double)parsedValue;
            }
            else if (value is null)
            {
                return 0.0;
            }

            // Если значение другого типа, пытаемся преобразовать
            try
            {
                int convertedValue = System.Convert.ToInt32(value);
                return (double)convertedValue;
            }
            catch
            {
                return 0.0;
            }
        }
    }
}
