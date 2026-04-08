using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChineseVocab.Models
{
    /// <summary>
    /// Модель колоды карточек.
    /// Соответствует таблице Decks в базе данных.
    /// </summary>
    public partial class Deck : ObservableObject
    {
        /// <summary>
        /// Уникальный идентификатор колоды (первичный ключ).
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Название колоды.
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// Описание колоды.
        /// </summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// Цвет колоды (для визуального выделения в UI).
        /// </summary>
        [ObservableProperty]
        private string _color = "#512BD4"; // Цвет по умолчанию (MAUI Primary)

        /// <summary>
        /// Иконка колоды (эмодзи или путь к изображению).
        /// </summary>
        [ObservableProperty]
        private string _icon = "📚";

        /// <summary>
        /// Уровень HSK, с которым связана колода (0 для смешанных колод).
        /// </summary>
        [ObservableProperty]
        private int _hskLevel = 0;

        /// <summary>
        /// Признак активной колоды (не удалена).
        /// </summary>
        [ObservableProperty]
        private bool _isActive = true;

        /// <summary>
        /// Признак системной колоды (нельзя удалить или изменить).
        /// </summary>
        [ObservableProperty]
        private bool _isSystem = false;

        /// <summary>
        /// Признак избранной колоды.
        /// </summary>
        [ObservableProperty]
        private bool _isFavorite = false;

        /// <summary>
        /// Дата создания колоды.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего изменения колоды.
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего изучения колоды.
        /// </summary>
        [ObservableProperty]
        private DateTime _lastStudied = DateTime.MinValue;

        /// <summary>
        /// Количество карточек в колоде (вычисляемое свойство).
        /// </summary>
        [ObservableProperty]
        private int _cardCount = 0;

        /// <summary>
        /// Количество изученных карточек в колоде.
        /// </summary>
        [ObservableProperty]
        private int _learnedCardCount = 0;

        /// <summary>
        /// Прогресс изучения колоды в процентах.
        /// </summary>
        [ObservableProperty]
        private double _progress = 0.0;

        /// <summary>
        /// Порядок сортировки колоды в списке.
        /// </summary>
        [ObservableProperty]
        private int _sortOrder = 0;

        /// <summary>
        /// Теги для категоризации колоды.
        /// </summary>
        [ObservableProperty]
        private string _tags = string.Empty;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Deck() { }

        /// <summary>
        /// Конструктор для быстрого создания колоды.
        /// </summary>
        public Deck(string name, string description = "", int hskLevel = 0)
        {
            Name = name;
            Description = description;
            HskLevel = hskLevel;
        }

        /// <summary>
        /// Обновляет прогресс изучения колоды на основе количества карточек.
        /// </summary>
        public void UpdateProgress()
        {
            if (CardCount > 0)
            {
                Progress = (double)LearnedCardCount / CardCount * 100.0;
            }
            else
            {
                Progress = 0.0;
            }
        }

        /// <summary>
        /// Возвращает строковое представление колоды (для отладки).
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({CardCount} карточек, {Progress:F1}%)";
        }
    }
}
