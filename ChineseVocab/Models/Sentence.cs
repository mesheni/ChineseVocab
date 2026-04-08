using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChineseVocab.Models
{
    /// <summary>
    /// Модель примера предложения с китайским текстом.
    /// Соответствует таблице Sentences в базе данных.
    /// </summary>
    public partial class Sentence : ObservableObject
    {
        /// <summary>
        /// Уникальный идентификатор предложения (первичный ключ).
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор карточки, к которой относится предложение.
        /// </summary>
        [Indexed]
        public int CardId { get; set; }

        /// <summary>
        /// Китайский текст предложения (иероглифы).
        /// </summary>
        [ObservableProperty]
        private string _chineseText = string.Empty;

        /// <summary>
        /// Пиньинь предложения (транскрипция).
        /// </summary>
        [ObservableProperty]
        private string _pinyin = string.Empty;

        /// <summary>
        /// Перевод предложения на русский язык.
        /// </summary>
        [ObservableProperty]
        private string _translation = string.Empty;

        /// <summary>
        /// Дополнительное объяснение или грамматические заметки.
        /// </summary>
        [ObservableProperty]
        private string _explanation = string.Empty;

        /// <summary>
        /// Уровень сложности предложения (1-5).
        /// </summary>
        [ObservableProperty]
        private int _difficultyLevel = 1;

        /// <summary>
        /// Источник предложения (например, "HSK 1", "Учебник", "Фильм").
        /// </summary>
        [ObservableProperty]
        private string _source = string.Empty;

        /// <summary>
        /// Теги для категоризации предложения.
        /// </summary>
        [ObservableProperty]
        private string _tags = string.Empty;

        /// <summary>
        /// Признак активного предложения (не удалено).
        /// </summary>
        [ObservableProperty]
        private bool _isActive = true;

        /// <summary>
        /// Дата создания предложения.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего изменения предложения.
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Количество показов предложения (для статистики).
        /// </summary>
        [ObservableProperty]
        private int _viewCount = 0;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Sentence() { }

        /// <summary>
        /// Конструктор для быстрого создания предложения.
        /// </summary>
        public Sentence(int cardId, string chineseText, string pinyin, string translation)
        {
            CardId = cardId;
            ChineseText = chineseText;
            Pinyin = pinyin;
            Translation = translation;
        }

        /// <summary>
        /// Конструктор для создания предложения со всеми основными полями.
        /// </summary>
        public Sentence(int cardId, string chineseText, string pinyin, string translation,
                        string explanation = "", int difficultyLevel = 1, string source = "")
        {
            CardId = cardId;
            ChineseText = chineseText;
            Pinyin = pinyin;
            Translation = translation;
            Explanation = explanation;
            DifficultyLevel = difficultyLevel;
            Source = source;
        }

        /// <summary>
        /// Увеличивает счетчик показов.
        /// </summary>
        public void IncrementViewCount()
        {
            ViewCount++;
        }

        /// <summary>
        /// Возвращает строковое представление предложения (для отладки).
        /// </summary>
        public override string ToString()
        {
            return $"{ChineseText} ({Pinyin}) - {Translation}";
        }
    }
}
