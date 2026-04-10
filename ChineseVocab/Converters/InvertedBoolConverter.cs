using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ChineseVocab.Converters
{
    /// <summary>
    /// Конвертер, который инвертирует логическое значение (true -> false, false -> true).
    /// Используется в XAML для изменения видимости элементов на основе обратной логики.
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует логическое значение в его противоположность.
        /// </summary>
        /// <param name="value">Логическое значение для инверсии.</param>
        /// <param name="targetType">Целевой тип (ожидается bool или nullable bool).</param>
        /// <param name="parameter">Дополнительный параметр (не используется).</param>
        /// <param name="culture">Информация о культуре (не используется).</param>
        /// <returns>Инвертированное логическое значение или null, если преобразование невозможно.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            if (value is null)
            {
                // null трактуется как false, инвертируется в true
                return true;
            }

            // Попробуем преобразовать другие типы
            try
            {
                bool converted = System.Convert.ToBoolean(value);
                return !converted;
            }
            catch
            {
                // Если преобразование невозможно, возвращаем false
                return false;
            }
        }

        /// <summary>
        /// Обратное преобразование (также инвертирует значение).
        /// </summary>
        /// <param name="value">Логическое значение для инверсии.</param>
        /// <param name="targetType">Целевой тип (ожидается bool или nullable bool).</param>
        /// <param name="parameter">Дополнительный параметр (не используется).</param>
        /// <param name="culture">Информация о культуре (не используется).</param>
        /// <returns>Инвертированное логическое значение или null, если преобразование невозможно.</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Обратное преобразование идентично прямому для инвертора
            return Convert(value, targetType, parameter, culture);
        }
    }
}
