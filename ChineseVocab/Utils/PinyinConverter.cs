using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ChineseVocab.Utils
{
    /// <summary>
    /// Статическая утилита для работы с пиньинем.
    /// Конвертация цифровых тонов в диакритические знаки,
    /// цветовое кодирование тонов, разбиение на слоги,
    /// сравнение произношения и форматирование для UI.
    /// </summary>
    public static class PinyinConverter
    {
        #region Константы

        /// <summary>
        /// Порядок приоритета гласных для расстановки знака тона.
        /// Правило: a > o > e > i > u > ü (первая найденная гласная получает тон).
        /// Исключения обрабатываются в логике размещения.
        /// </summary>
        private const string VowelPriority = "aeoiuü";

        /// <summary>
        /// Все гласные, используемые в пиньине.
        /// </summary>
        private const string Vowels = "aeiouüAEIOUÜ";

        /// <summary>
        /// Расширенный набор гласных для детектирования в слогах (включая диакритические).
        /// </summary>
        private const string VowelsExtended = "aeiouüāáǎàēéěèīíǐìōóǒòūúǔùǖǘǚǜAEIOUÜĀÁǍÀĒÉĚÈĪÍǏÌŌÓǑÒŪÚǓÙǕǗǙǛ";

        /// <summary>
        /// Инициали (начальные согласные) пиньиня — используются для разбиения слогов.
        /// </summary>
        private static readonly HashSet<string> Initials = new(StringComparer.OrdinalIgnoreCase)
        {
            "b", "p", "m", "f", "d", "t", "n", "l",
            "g", "k", "h", "j", "q", "x",
            "zh", "ch", "sh", "r", "z", "c", "s",
            "y", "w"
        };

        // Цвета для пяти тонов (современная палитра, доступная для дальтоников)
        private static readonly Color[] ToneColors =
        [
            Color.FromArgb("#E53935"), // Тон 1 (¯) — красный
            Color.FromArgb("#FB8C00"), // Тон 2 (´) — оранжевый
            Color.FromArgb("#43A047"), // Тон 3 (ˇ) — зелёный
            Color.FromArgb("#1E88E5"), // Тон 4 (ˋ) — синий
            Color.FromArgb("#9E9E9E"), // Тон 5 (нейтральный) — серый
        ];

        // Unicode-коды для диакритических знаков по тонам
        private static readonly Dictionary<char, string> ToneDiacritics = new()
        {
            ['1'] = "\u0304", //  ̄  (macron, тон 1)
            ['2'] = "\u0301", //  ́  (acute, тон 2)
            ['3'] = "\u030C", //  ̌  (caron, тон 3)
            ['4'] = "\u0300", //  ̀  (grave, тон 4)
        };

        #endregion

        #region Таблицы преобразования гласных

        /// <summary>
        /// Карта преобразования гласной + тон → готовая буква с диакритикой.
        /// Ключ: "гласная:тон" (строчная). Значение: буква с диакритикой.
        /// </summary>
        private static readonly Dictionary<string, char> VowelToneMap = new()
        {
            // Тон 1 (macron)
            ["a:1"] = 'ā',
            ["e:1"] = 'ē',
            ["i:1"] = 'ī',
            ["o:1"] = 'ō',
            ["u:1"] = 'ū',
            ["ü:1"] = 'ǖ',
            ["A:1"] = 'Ā',
            ["E:1"] = 'Ē',
            ["I:1"] = 'Ī',
            ["O:1"] = 'Ō',
            ["U:1"] = 'Ū',
            ["Ü:1"] = 'Ǖ',

            // Тон 2 (acute)
            ["a:2"] = 'á',
            ["e:2"] = 'é',
            ["i:2"] = 'í',
            ["o:2"] = 'ó',
            ["u:2"] = 'ú',
            ["ü:2"] = 'ǘ',
            ["A:2"] = 'Á',
            ["E:2"] = 'É',
            ["I:2"] = 'Í',
            ["O:2"] = 'Ó',
            ["U:2"] = 'Ú',
            ["Ü:2"] = 'Ǘ',

            // Тон 3 (caron)
            ["a:3"] = 'ǎ',
            ["e:3"] = 'ě',
            ["i:3"] = 'ǐ',
            ["o:3"] = 'ǒ',
            ["u:3"] = 'ǔ',
            ["ü:3"] = 'ǚ',
            ["A:3"] = 'Ǎ',
            ["E:3"] = 'Ě',
            ["I:3"] = 'Ǐ',
            ["O:3"] = 'Ǒ',
            ["U:3"] = 'Ǔ',
            ["Ü:3"] = 'Ǚ',

            // Тон 4 (grave)
            ["a:4"] = 'à',
            ["e:4"] = 'è',
            ["i:4"] = 'ì',
            ["o:4"] = 'ò',
            ["u:4"] = 'ù',
            ["ü:4"] = 'ǜ',
            ["A:4"] = 'À',
            ["E:4"] = 'È',
            ["I:4"] = 'Ì',
            ["O:4"] = 'Ò',
            ["U:4"] = 'Ù',
            ["Ü:4"] = 'Ǜ',
        };

        #endregion

        #region Основной метод: цифровые тоны → диакритические знаки

        /// <summary>
        /// Конвертирует пиньинь с цифровыми тонами в пиньинь с диакритическими знаками.
        /// Пример: "ni3 hao3" → "nǐ hǎo", "zhong1 guo2" → "zhōng guó".
        ///
        /// Формат ввода: слоги, разделённые пробелами. Каждый слог оканчивается цифрой 1-5.
        /// Цифра 5 (нейтральный тон) — знак тона не добавляется, цифра удаляется.
        /// Слоги без цифры в конце остаются без изменений.
        /// </summary>
        /// <param name="numberedPinyin">Пиньинь с цифровыми тонами (например, "ni3 hao3").</param>
        /// <returns>Пиньинь с диакритическими знаками ("nǐ hǎo").</returns>
        public static string NumberedToDiacritics(string numberedPinyin)
        {
            if (string.IsNullOrWhiteSpace(numberedPinyin))
                return string.Empty;

            var parts = numberedPinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) result.Append(' ');
                result.Append(ConvertSyllable(parts[i]));
            }

            return result.ToString();
        }

        /// <summary>
        /// Конвертирует один слог пиньиня с цифровым тоном в слог с диакритикой.
        /// </summary>
        private static string ConvertSyllable(string syllable)
        {
            if (string.IsNullOrEmpty(syllable))
                return syllable;

            // Извлекаем тон (последний символ, если это цифра 1-5)
            char lastChar = syllable[^1];
            int tone;

            if (char.IsDigit(lastChar) && lastChar >= '1' && lastChar <= '5')
            {
                tone = lastChar - '0';
                syllable = syllable[..^1]; // Убираем цифру тона
            }
            else
            {
                // Нет цифрового тона — возвращаем как есть
                return syllable;
            }

            // Нейтральный тон — просто возвращаем слог без цифры
            if (tone == 5)
                return syllable;

            // Находим позицию гласной для установки знака тона
            int vowelIndex = FindToneVowelIndex(syllable);

            if (vowelIndex < 0)
                return syllable; // Нет гласных — возвращаем как есть

            // Заменяем гласную на версию с диакритикой
            char vowel = syllable[vowelIndex];
            string mapKey = $"{vowel}:{tone}";

            if (VowelToneMap.TryGetValue(mapKey, out char tonedVowel))
            {
                char[] chars = syllable.ToCharArray();
                chars[vowelIndex] = tonedVowel;
                return new string(chars);
            }

            // Если готовой буквы нет — добавляем комбинирующий диакритический знак
            if (ToneDiacritics.TryGetValue((char)('0' + tone), out string? diacritic))
            {
                return syllable.Insert(vowelIndex + 1, diacritic);
            }

            return syllable;
        }

        /// <summary>
        /// Определяет индекс гласной в слоге, на которую нужно поставить знак тона.
        ///
        /// Правила:
        /// 1. Гласная 'a' (или 'e') получает тон в первую очередь.
        /// 2. В сочетании 'ou' тон ставится на 'o'.
        /// 3. Иначе тон ставится на вторую гласную (или последнюю, если их больше двух).
        /// 4. Особый случай: 'iu' — тон на 'u'; 'ui' — тон на 'i'.
        /// </summary>
        private static int FindToneVowelIndex(string syllable)
        {
            // Собираем все индексы гласных (включая ü)
            List<int> vowelIndices = [];
            for (int i = 0; i < syllable.Length; i++)
            {
                if (Vowels.IndexOf(syllable[i]) >= 0)
                    vowelIndices.Add(i);
            }

            if (vowelIndices.Count == 0)
                return -1;

            if (vowelIndices.Count == 1)
                return vowelIndices[0];

            // Правило 1: первая по порядку 'a' или 'e' получает тон
            foreach (int idx in vowelIndices)
            {
                char c = char.ToLowerInvariant(syllable[idx]);
                if (c == 'a' || c == 'e')
                    return idx;
            }

            // Правило 2: 'ou' → тон на 'o'
            string lower = syllable.ToLowerInvariant();
            if (lower.Contains("ou"))
            {
                int oIdx = vowelIndices.Find(i => char.ToLowerInvariant(syllable[i]) == 'o');
                if (oIdx >= 0) return oIdx;
            }

            // Правило 3: тон на вторую гласную
            // Это покрывает: iu→u, ui→i, а также общий случай двух гласных
            return vowelIndices[^1];
        }

        #endregion

        #region Извлечение тонов

        /// <summary>
        /// Извлекает номера тонов из пиньиня с диакритическими знаками.
        /// "nǐ hǎo" → [3, 3].
        /// Нейтральный тон (без знака) → 5.
        /// </summary>
        /// <param name="pinyin">Пиньинь с диакритическими знаками.</param>
        /// <returns>Массив номеров тонов (1-5).</returns>
        public static int[] ExtractTones(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
                return [];

            var parts = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var tones = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                tones[i] = ExtractSyllableTone(parts[i]);
            }

            return tones;
        }

        /// <summary>
        /// Определяет тон одного слога по диакритическому знаку.
        /// Возвращает 1-5 (5 = нейтральный тон / нет знака).
        /// </summary>
        public static int ExtractSyllableTone(string syllable)
        {
            if (string.IsNullOrEmpty(syllable))
                return 5;

            // Таблица: диакритическая гласная → тон
            // Тон 1
            if (ContainsAny(syllable, "āēīōūǖĀĒĪŌŪǕ"))
                return 1;

            // Тон 2
            if (ContainsAny(syllable, "áéíóúǘÁÉÍÓÚǗ"))
                return 2;

            // Тон 3
            if (ContainsAny(syllable, "ǎěǐǒǔǚǍĚǏǑǓǙ"))
                return 3;

            // Тон 4
            if (ContainsAny(syllable, "àèìòùǜÀÈÌÒÙǛ"))
                return 4;

            // Нейтральный тон (включая слоги с цифрой 5 на конце)
            return 5;
        }

        private static bool ContainsAny(string text, string chars)
        {
            foreach (char c in chars)
            {
                if (text.Contains(c))
                    return true;
            }
            return false;
        }

        #endregion

        #region Цветовое кодирование тонов

        /// <summary>
        /// Возвращает цвет, соответствующий тону (1-5).
        /// Тон 1 → красный, 2 → оранжевый, 3 → зелёный, 4 → синий, 5 → серый.
        /// </summary>
        public static Color GetToneColor(int tone)
        {
            int index = Math.Clamp(tone - 1, 0, ToneColors.Length - 1);
            return ToneColors[index];
        }

        /// <summary>
        /// Возвращает цвет для указанного слога на основе его диакритического знака.
        /// </summary>
        public static Color GetSyllableToneColor(string syllable)
        {
            int tone = ExtractSyllableTone(syllable);
            return GetToneColor(tone);
        }

        /// <summary>
        /// Возвращает HTML-цвет (hex) для тона. Удобно для использования в XAML-конвертерах.
        /// </summary>
        public static string GetToneColorHex(int tone)
        {
            return tone switch
            {
                1 => "#E53935",
                2 => "#FB8C00",
                3 => "#43A047",
                4 => "#1E88E5",
                _ => "#9E9E9E",
            };
        }

        /// <summary>
        /// Возвращает символ-обозначение тона для отладки.
        /// </summary>
        public static string GetToneSymbol(int tone)
        {
            return tone switch
            {
                1 => " ̄  (первый)",
                2 => " ́  (второй)",
                3 => " ̌  (третий)",
                4 => " ̀  (четвёртый)",
                5 => "· (нейтральный)",
                _ => "? (неизвестный)",
            };
        }

        #endregion

        #region Разбиение на слоги

        /// <summary>
        /// Разбивает пиньинь (с диакритикой или без) на отдельные слоги.
        /// Обрабатывает слитный пиньинь без пробелов: "nǐhǎo" → ["nǐ", "hǎo"].
        /// Пиньинь с пробелами просто разбивается по пробелам.
        /// </summary>
        /// <param name="pinyin">Пиньинь (слитный или с пробелами).</param>
        /// <returns>Массив слогов.</returns>
        public static string[] SplitSyllables(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
                return [];

            // Если уже разделён пробелами — просто разбиваем
            if (pinyin.Contains(' '))
                return pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Слитный пиньинь — разбиваем по правилам
            return SplitContinuousPinyin(pinyin);
        }

        /// <summary>
        /// Разбивает слитную строку пиньиня на слоги.
        /// Алгоритм: сканируем слева направо, пытаясь выделить максимальный
        /// известный инициаль + финаль.
        /// </summary>
        private static string[] SplitContinuousPinyin(string pinyin)
        {
            var syllables = new List<string>();
            int pos = 0;

            while (pos < pinyin.Length)
            {
                int syllableStart = pos;

                // Пропускаем апостроф (используется для разделения слогов: Xi'an)
                if (pinyin[pos] == '\'')
                {
                    pos++;
                    syllableStart = pos;
                }

                // Проверяем, является ли начало известным инициальем
                string? initial = null;
                if (pos + 1 < pinyin.Length)
                {
                    string twoChar = pinyin.Substring(pos, 2);
                    if (Initials.Contains(twoChar))
                    {
                        initial = twoChar;
                        pos += 2;
                    }
                }

                if (initial == null)
                {
                    string oneChar = pinyin[pos].ToString();
                    if (Initials.Contains(oneChar))
                    {
                        initial = oneChar;
                        pos++;
                    }
                }

                // Ищем конец финали (пока есть гласные и допустимые окончания)
                while (pos < pinyin.Length)
                {
                    char c = pinyin[pos];

                    if (IsPinyinVowel(c))
                    {
                        pos++;
                        continue;
                    }

                    // Допустимые окончания слога после гласных: n, ng, r
                    if (c == 'n')
                    {
                        pos++;
                        // Проверяем 'ng'
                        if (pos < pinyin.Length && pinyin[pos] == 'g')
                            pos++;
                        break;
                    }

                    if (c == 'r')
                    {
                        pos++;
                        break;
                    }

                    // Не гласная и не окончание — конец слога
                    break;
                }

                int syllableEnd = pos;
                string syllable = pinyin[syllableStart..syllableEnd];

                if (!string.IsNullOrEmpty(syllable))
                    syllables.Add(syllable);
            }

            return [.. syllables];
        }

        /// <summary>
        /// Проверяет, является ли символ гласной пиньиня (включая диакритические варианты).
        /// </summary>
        private static bool IsPinyinVowel(char c)
        {
            return VowelsExtended.IndexOf(c) >= 0;
        }

        #endregion

        #region Форматирование для UI

        /// <summary>
        /// Создаёт FormattedString с цветовым кодированием тонов для отображения в MAUI Label.
        /// Каждый слог окрашивается в цвет своего тона.
        ///
        /// Пример использования в XAML через биндинг или в code-behind:
        ///   label.FormattedText = PinyinConverter.FormatWithToneColors("nǐ hǎo");
        /// </summary>
        /// <param name="pinyin">Пиньинь с диакритическими знаками.</param>
        /// <returns>FormattedString с цветными слогами.</returns>
        public static FormattedString FormatWithToneColors(string pinyin)
        {
            var formatted = new FormattedString();

            if (string.IsNullOrWhiteSpace(pinyin))
                return formatted;

            var parts = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    formatted.Spans.Add(new Span
                    {
                        Text = " ",
                        FontSize = 16
                    });
                }

                int tone = ExtractSyllableTone(parts[i]);
                Color color = GetToneColor(tone);

                formatted.Spans.Add(new Span
                {
                    Text = parts[i],
                    TextColor = color,
                    FontSize = 16
                });
            }

            return formatted;
        }

        /// <summary>
        /// Создаёт строку с аннотированными тонами для отладки.
        /// "nǐ hǎo" → "ni(3) hao(3)"
        /// </summary>
        public static string ToAnnotatedString(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
                return string.Empty;

            var parts = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) result.Append(' ');
                int tone = ExtractSyllableTone(parts[i]);
                // Убираем диакритику для аннотированного вывода
                string plain = StripDiacritics(parts[i]);
                result.Append($"{plain}({tone})");
            }

            return result.ToString();
        }

        /// <summary>
        /// Убирает диакритические знаки из пиньиня, оставляя только базовые латинские буквы.
        /// "nǐ hǎo" → "ni hao"
        /// </summary>
        public static string StripDiacritics(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
                return string.Empty;

            var result = new StringBuilder();
            foreach (char c in pinyin)
            {
                result.Append(c switch
                {
                    // Тон 1
                    'ā' => 'a',
                    'Ā' => 'A',
                    'ē' => 'e',
                    'Ē' => 'E',
                    'ī' => 'i',
                    'Ī' => 'I',
                    'ō' => 'o',
                    'Ō' => 'O',
                    'ū' => 'u',
                    'Ū' => 'U',
                    'ǖ' => 'ü',
                    'Ǖ' => 'Ü',

                    // Тон 2
                    'á' => 'a',
                    'Á' => 'A',
                    'é' => 'e',
                    'É' => 'E',
                    'í' => 'i',
                    'Í' => 'I',
                    'ó' => 'o',
                    'Ó' => 'O',
                    'ú' => 'u',
                    'Ú' => 'U',
                    'ǘ' => 'ü',
                    'Ǘ' => 'Ü',

                    // Тон 3
                    'ǎ' => 'a',
                    'Ǎ' => 'A',
                    'ě' => 'e',
                    'Ě' => 'E',
                    'ǐ' => 'i',
                    'Ǐ' => 'I',
                    'ǒ' => 'o',
                    'Ǒ' => 'O',
                    'ǔ' => 'u',
                    'Ǔ' => 'U',
                    'ǚ' => 'ü',
                    'Ǚ' => 'Ü',

                    // Тон 4
                    'à' => 'a',
                    'À' => 'A',
                    'è' => 'e',
                    'È' => 'E',
                    'ì' => 'i',
                    'Ì' => 'I',
                    'ò' => 'o',
                    'Ò' => 'O',
                    'ù' => 'u',
                    'Ù' => 'U',
                    'ǜ' => 'ü',
                    'Ǜ' => 'Ü',

                    _ => c
                });
            }

            return result.ToString();
        }

        #endregion

        #region Сравнение произношения

        /// <summary>
        /// Сравнивает два варианта пиньиня и возвращает оценку схожести от 0.0 до 1.0.
        /// Использует расстояние Левенштейна, нормализованное к длине строк.
        /// Предварительно приводит к нижнему регистру и убирает диакритику.
        ///
        /// Может использоваться для проверки произношения пользователя:
        ///   ComparePronunciation(userInput, correctPinyin) > 0.8 → приемлемо.
        /// </summary>
        /// <param name="userPinyin">Пиньинь, введённый пользователем.</param>
        /// <param name="correctPinyin">Правильный пиньинь (с тонами или без).</param>
        /// <returns>Оценка схожести от 0.0 (совсем не похоже) до 1.0 (идентично).</returns>
        public static double ComparePronunciation(string userPinyin, string correctPinyin)
        {
            if (string.IsNullOrWhiteSpace(userPinyin) && string.IsNullOrWhiteSpace(correctPinyin))
                return 1.0;

            if (string.IsNullOrWhiteSpace(userPinyin) || string.IsNullOrWhiteSpace(correctPinyin))
                return 0.0;

            // Нормализуем: нижний регистр, без диакритики, без пробелов
            string user = NormalizeForComparison(userPinyin);
            string correct = NormalizeForComparison(correctPinyin);

            if (user == correct)
                return 1.0;

            // Расстояние Левенштейна
            int distance = LevenshteinDistance(user, correct);
            int maxLen = Math.Max(user.Length, correct.Length);

            if (maxLen == 0)
                return 1.0;

            return 1.0 - (double)distance / maxLen;
        }

        /// <summary>
        /// Сравнивает тоны двух вариантов пиньиня.
        /// Возвращает процент совпадающих тонов (0.0 – 1.0).
        /// "nǐ hǎo" vs "ni hao" → оба имеют тон 3, результат 1.0.
        /// "mā ma" vs "mā mǎ" → 1 из 2 совпадает, результат 0.5.
        /// </summary>
        public static double CompareTones(string userPinyin, string correctPinyin)
        {
            int[] userTones = ExtractTones(userPinyin);
            int[] correctTones = ExtractTones(correctPinyin);

            if (correctTones.Length == 0)
                return userTones.Length == 0 ? 1.0 : 0.0;

            // Выравниваем по длине корректного варианта
            int matchCount = 0;
            for (int i = 0; i < Math.Min(userTones.Length, correctTones.Length); i++)
            {
                if (userTones[i] == correctTones[i])
                    matchCount++;
            }

            return (double)matchCount / correctTones.Length;
        }

        /// <summary>
        /// Нормализует строку пиньиня для сравнения:
        /// нижний регистр, удаление диакритики, удаление пробелов.
        /// </summary>
        private static string NormalizeForComparison(string pinyin)
        {
            string stripped = StripDiacritics(pinyin);
            // Убираем пробелы и приводим к нижнему регистру
            return Regex.Replace(stripped, @"\s+", "").ToLowerInvariant();
        }

        /// <summary>
        /// Вычисляет расстояние Левенштейна между двумя строками.
        /// </summary>
        private static int LevenshteinDistance(string a, string b)
        {
            int lenA = a.Length;
            int lenB = b.Length;

            // Оптимизация: использовать одномерный массив
            int[] prev = new int[lenB + 1];
            int[] curr = new int[lenB + 1];

            for (int j = 0; j <= lenB; j++)
                prev[j] = j;

            for (int i = 1; i <= lenA; i++)
            {
                curr[0] = i;

                for (int j = 1; j <= lenB; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    curr[j] = Math.Min(
                        Math.Min(curr[j - 1] + 1, prev[j] + 1),
                        prev[j - 1] + cost
                    );
                }

                (prev, curr) = (curr, prev);
            }

            return prev[lenB];
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет, является ли строка корректным пиньинем с диакритическими знаками.
        /// </summary>
        public static bool IsValidDiacriticPinyin(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
                return false;

            // Каждый слог должен содержать хотя бы одну гласную (включая диакритические)
            var parts = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.All(part => part.Any(c => IsPinyinVowel(c)));
        }

        /// <summary>
        /// Проверяет, является ли строка корректным пиньинем с цифровыми тонами.
        /// Пример: "ni3 hao3" → true, "hello" → false.
        /// </summary>
        public static bool IsValidNumberedPinyin(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
                return false;

            var parts = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.All(part =>
            {
                // Слог должен содержать гласные и оканчиваться цифрой 1-5
                if (part.Length < 2) return false;
                char last = part[^1];
                if (last < '1' || last > '5') return false;
                string withoutTone = part[..^1];
                return withoutTone.Any(c => Vowels.IndexOf(c) >= 0);
            });
        }

        /// <summary>
        /// Конвертирует диакритический пиньинь обратно в цифровой формат.
        /// "nǐ hǎo" → "ni3 hao3"
        /// </summary>
        public static string DiacriticsToNumbered(string pinyin)
        {
            if (string.IsNullOrWhiteSpace(pinyin))
                return string.Empty;

            var parts = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) result.Append(' ');
                int tone = ExtractSyllableTone(parts[i]);
                string plain = StripDiacritics(parts[i]);
                result.Append(plain);
                if (tone >= 1 && tone <= 5)
                    result.Append(tone);
            }

            return result.ToString();
        }

        #endregion
    }
}
