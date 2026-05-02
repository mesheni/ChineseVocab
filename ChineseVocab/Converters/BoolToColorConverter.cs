using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ChineseVocab.Converters
{
    /// <summary>
    /// Конвертер, который преобразует логическое значение в цвет:
    /// true  → зелёный (правильный ответ)
    /// false → красный (неправильный ответ)
    /// null  → серый (нейтральный)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>
        /// Цвет для правильного ответа (true).
        /// </summary>
        private static readonly Color TrueColor = Color.FromArgb("#10B981");    // Зелёный

        /// <summary>
        /// Цвет для неправильного ответа (false).
        /// </summary>
        private static readonly Color FalseColor = Color.FromArgb("#EF4444");   // Красный

        /// <summary>
        /// Цвет по умолчанию, если значение не bool или null.
        /// </summary>
        private static readonly Color DefaultColor = Color.FromArgb("#6B7280"); // Серый

        /// <summary>
        /// Преобразует bool (IsAnswerCorrect) в Color для TextColor сообщения.
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                true => TrueColor,
                false => FalseColor,
                null => DefaultColor,
                _ => DefaultColor
            };
        }

        /// <summary>
        /// Обратное преобразование не требуется.
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
