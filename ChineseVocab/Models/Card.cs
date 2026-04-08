using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChineseVocab.Models
{
    /// <summary>
    /// Модель карточки с китайским иероглифом или словом.
    /// Соответствует таблице Cards в базе данных.
    /// </summary>
    public partial class Card : ObservableObject
    {
        /// <summary>
        /// Уникальный идентификатор карточки (первичный ключ).
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Иероглиф (китайский символ).
        /// </summary>
        [ObservableProperty]
        private string _character = string.Empty;

        /// <summary>
        /// Упрощенный вариант иероглифа (для упрощенного китайского).
        /// </summary>
        [ObservableProperty]
        private string _simplified = string.Empty;

        /// <summary>
        /// Традиционный вариант иероглифа (для традиционного китайского).
        /// </summary>
        [ObservableProperty]
        private string _traditional = string.Empty;

        /// <summary>
        /// Пиньинь (транскрипция произношения).
        /// </summary>
        [ObservableProperty]
        private string _pinyin = string.Empty;

        /// <summary>
        /// Перевод или определение на русском языке.
        /// </summary>
        [ObservableProperty]
        private string _definition = string.Empty;

        /// <summary>
        /// Уровень HSK (1-9), где 0 означает не входит в HSK.
        /// </summary>
        [ObservableProperty]
        private int _hskLevel = 0;

        /// <summary>
        /// Количество черт в иероглифе.
        /// </summary>
        [ObservableProperty]
        private int _strokeCount = 0;

        /// <summary>
        /// Радикал (ключевой компонент) иероглифа.
        /// </summary>
        [ObservableProperty]
        private string _radical = string.Empty;

        /// <summary>
        /// Тип иероглифа согласно классификации (пиктограмма, идеограмма и т.д.).
        /// </summary>
        [ObservableProperty]
        private string _characterType = string.Empty;

        /// <summary>
        /// Компоненты, из которых состоит иероглиф (JSON-строка или список через разделитель).
        /// </summary>
        [ObservableProperty]
        private string _components = string.Empty;

        /// <summary>
        /// Порядок черт в формате SVG или специальной нотации.
        /// </summary>
        [ObservableProperty]
        private string _strokeOrder = string.Empty;

        /// <summary>
        /// Дата создания карточки.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего изменения карточки.
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Признак активной карточки (не удалена).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Теги для категоризации (через запятую или JSON).
        /// </summary>
        [ObservableProperty]
        private string _tags = string.Empty;

        /// <summary>
        /// Частота использования иероглифа (на основе корпуса текстов).
        /// </summary>
        [ObservableProperty]
        private int _frequencyRank = 0;

        /// <summary>
        /// Дополнительные заметки или комментарии.
        /// </summary>
        [ObservableProperty]
        private string _notes = string.Empty;

        /// <summary>
        /// URL изображения или анимации порядка черт (опционально).
        /// </summary>
        [ObservableProperty]
        private string _imageUrl = string.Empty;

        /// <summary>
        /// Аудио URL произношения (опционально).
        /// </summary>
        [ObservableProperty]
        private string _audioUrl = string.Empty;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Card() { }

        /// <summary>
        /// Конструктор для быстрого создания карточки.
        /// </summary>
        public Card(string character, string pinyin, string definition, int hskLevel = 0)
        {
            Character = character;
            Pinyin = pinyin;
            Definition = definition;
            HskLevel = hskLevel;
            Simplified = character; // По умолчанию упрощенный такой же
            Traditional = character; // По умолчанию традиционный такой же
        }

        /// <summary>
        /// Возвращает строковое представление карточки (для отладки).
        /// </summary>
        public override string ToString()
        {
            return $"{Character} ({Pinyin}) - {Definition}";
        }
    }
}
