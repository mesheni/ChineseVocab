using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ChineseVocab.Converters
{
    /// <summary>
    /// Конвертер, который преобразует режим изучения (StudyMode) в цвет для визуального выделения.
    /// Если текущий режим совпадает с параметром - возвращает активный цвет, иначе неактивный.
    /// </summary>
    public class StudyModeToColorConverter : IValueConverter
    {
        /// <summary>
        /// Активный цвет для выбранного режима изучения.
        /// </summary>
        private static readonly Color ActiveColor = Color.FromArgb("#512BD4"); // Primary цвет

        /// <summary>
        /// Неактивный цвет для невыбранного режима изучения.
        /// </summary>
        private static readonly Color InactiveColor = Color.FromArgb("#6C757D"); // Secondary цвет

        /// <summary>
        /// Преобразует текущий режим изучения в цвет на основе параметра.
        /// </summary>
        /// <param name="value">Текущий режим изучения (StudyMode).</param>
        /// <param name="targetType">Целевой тип (ожидается Color).</param>
        /// <param name="parameter">Параметр - строковое представление режима для сравнения ("New", "Review", "Mixed").</param>
        /// <param name="culture">Информация о культуре (не используется).</param>
        /// <returns>Активный цвет, если режим совпадает с параметром, иначе неактивный цвет.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || parameter is null)
                return InactiveColor;

            try
            {
                // Получаем текущий режим как строку
                string currentModeStr = value.ToString();
                string targetModeStr = parameter.ToString();

                // Сравниваем строки (регистр не учитываем)
                if (string.Equals(currentModeStr, targetModeStr, StringComparison.OrdinalIgnoreCase))
                {
                    return ActiveColor;
                }
                else
                {
                    return InactiveColor;
                }
            }
            catch
            {
                // В случае ошибки возвращаем неактивный цвет
                return InactiveColor;
            }
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
