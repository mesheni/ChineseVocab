using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChineseVocab.Models
{
    /// <summary>
    /// Модель типа иероглифа согласно традиционной китайской классификации.
    /// Соответствует таблице CharacterTypes в базе данных.
    /// </summary>
    public partial class CharacterType : ObservableObject
    {
        /// <summary>
        /// Уникальный идентификатор типа (первичный ключ).
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Название типа на русском языке.
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// Название типа на китайском языке (иероглифы).
        /// </summary>
        [ObservableProperty]
        private string _chineseName = string.Empty;

        /// <summary>
        /// Пиньинь названия типа.
        /// </summary>
        [ObservableProperty]
        private string _pinyin = string.Empty;

        /// <summary>
        /// Подробное описание типа иероглифа.
        /// </summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// Категория типа (основные 6 типов: пиктограммы, указательные и т.д.).
        /// </summary>
        [ObservableProperty]
        private string _category = string.Empty;

        /// <summary>
        /// Примеры иероглифов данного типа (через запятую или JSON).
        /// </summary>
        [ObservableProperty]
        private string _examples = string.Empty;

        /// <summary>
        /// Объяснение принципа формирования иероглифов данного типа.
        /// </summary>
        [ObservableProperty]
        private string _formationPrinciple = string.Empty;

        /// <summary>
        /// Особенности написания или структурные паттерны.
        /// </summary>
        [ObservableProperty]
        private string _strokePattern = string.Empty;

        /// <summary>
        /// Процент иероглифов данного типа от общего количества (приблизительно).
        /// </summary>
        [ObservableProperty]
        private double _percentage = 0.0;

        /// <summary>
        /// Уровень сложности изучения (1-5, где 1 - самый простой).
        /// </summary>
        [ObservableProperty]
        private int _difficultyLevel = 1;

        /// <summary>
        /// Иконка для визуального представления типа (эмодзи или путь к изображению).
        /// </summary>
        [ObservableProperty]
        private string _icon = "🔤";

        /// <summary>
        /// Цвет для визуального выделения типа в UI.
        /// </summary>
        [ObservableProperty]
        private string _color = "#512BD4";

        /// <summary>
        /// Источник информации о типе (ссылка или название книги).
        /// </summary>
        [ObservableProperty]
        private string _source = string.Empty;

        /// <summary>
        /// Признак активного типа (не удален).
        /// </summary>
        [ObservableProperty]
        private bool _isActive = true;

        /// <summary>
        /// Признак системного типа (нельзя удалить или изменить).
        /// </summary>
        [ObservableProperty]
        private bool _isSystem = false;

        /// <summary>
        /// Дата создания записи.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего изменения записи.
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public CharacterType() { }

        /// <summary>
        /// Конструктор для быстрого создания типа иероглифа.
        /// </summary>
        public CharacterType(string name, string chineseName, string pinyin, string category, string description)
        {
            Name = name;
            ChineseName = chineseName;
            Pinyin = pinyin;
            Category = category;
            Description = description;
        }

        /// <summary>
        /// Конструктор для создания полного типа иероглифа.
        /// </summary>
        public CharacterType(string name, string chineseName, string pinyin, string category,
                            string description, string examples, string formationPrinciple,
                            double percentage, int difficultyLevel)
        {
            Name = name;
            ChineseName = chineseName;
            Pinyin = pinyin;
            Category = category;
            Description = description;
            Examples = examples;
            FormationPrinciple = formationPrinciple;
            Percentage = percentage;
            DifficultyLevel = difficultyLevel;
        }

        /// <summary>
        /// Создает предустановленные типы иероглифов согласно традиционной классификации.
        /// </summary>
        public static List<CharacterType> GetDefaultCharacterTypes()
        {
            return new List<CharacterType>
            {
                new CharacterType(
                    name: "Пиктограммы",
                    chineseName: "象形字",
                    pinyin: "xiàngxíngzì",
                    category: "Основной",
                    description: "Иероглифы, изображающие конкретные предметы или явления. Самые древние и наглядные символы.",
                    examples: "人 (человек), 日 (солнце), 月 (луна), 山 (гора), 水 (вода)",
                    formationPrinciple: "Изображение предмета в упрощенной форме",
                    percentage: 4.0,
                    difficultyLevel: 1
                )
                {
                    Icon = "🖼️",
                    Color = "#4CAF50"
                },

                new CharacterType(
                    name: "Указательные",
                    chineseName: "指事字",
                    pinyin: "zhǐshìzì",
                    category: "Основной",
                    description: "Иероглифы, указывающие на абстрактные понятия или положения. Часто добавляют специальные знаки к пиктограммам.",
                    examples: "上 (верх), 下 (низ), 一 (один), 二 (два), 三 (три)",
                    formationPrinciple: "Указание на абстрактное понятие с помощью знаков",
                    percentage: 1.0,
                    difficultyLevel: 2
                )
                {
                    Icon = "☝️",
                    Color = "#2196F3"
                },

                new CharacterType(
                    name: "Идеограммы",
                    chineseName: "会意字",
                    pinyin: "huìyìzì",
                    category: "Основной",
                    description: "Иероглифы, составленные из двух или более компонентов, значение которых объединяется для создания нового смысла.",
                    examples: "明 (светлый = солнце + луна), 休 (отдыхать = человек + дерево), 好 (хороший = женщина + ребенок)",
                    formationPrinciple: "Комбинирование значений нескольких компонентов",
                    percentage: 13.0,
                    difficultyLevel: 3
                )
                {
                    Icon = "💡",
                    Color = "#FF9800"
                },

                new CharacterType(
                    name: "Фоноидеограммы",
                    chineseName: "形声字",
                    pinyin: "xíngshēngzì",
                    category: "Основной",
                    description: "Самый распространенный тип. Состоят из смыслового компонента (радикала) и фонетического компонента (указывает на произношение).",
                    examples: "妈 (мама = женщина + ма), 请 (просить = речь + цин), 河 (река = вода + кэ)",
                    formationPrinciple: "Радикал (значение) + фонетик (звучание)",
                    percentage: 80.0,
                    difficultyLevel: 4
                )
                {
                    Icon = "🎵",
                    Color = "#9C27B0"
                },

                new CharacterType(
                    name: "Заимствованные",
                    chineseName: "假借字",
                    pinyin: "jiǎjièzì",
                    category: "Дополнительный",
                    description: "Иероглифы, первоначально созданные для одного значения, но позже заимствованные для другого значения с похожим звучанием.",
                    examples: "来 (первоначально: пшеница, сейчас: приходить), 我 (первоначально: оружие, сейчас: я)",
                    formationPrinciple: "Заимствование по звучанию",
                    percentage: 1.0,
                    difficultyLevel: 5
                )
                {
                    Icon = "🔀",
                    Color = "#F44336"
                },

                new CharacterType(
                    name: "Производные",
                    chineseName: "转注字",
                    pinyin: "zhuǎnzhùzì",
                    category: "Дополнительный",
                    description: "Иероглифы, образованные путем изменения или адаптации существующих иероглифов для родственных понятий.",
                    examples: "老 (старый) и 考 (экзамен, проверять), 首 (голова) и 頁 (страница, лист)",
                    formationPrinciple: "Производные от существующих иероглифов",
                    percentage: 1.0,
                    difficultyLevel: 5
                )
                {
                    Icon = "🔄",
                    Color = "#607D8B"
                }
            };
        }

        /// <summary>
        /// Возвращает строковое представление типа иероглифа (для отладки).
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({ChineseName}, {Pinyin}) - {Category}";
        }
    }
}
