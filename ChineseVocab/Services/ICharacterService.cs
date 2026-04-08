using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Интерфейс для сервиса работы с китайскими иероглифами.
    /// Предоставляет методы для получения информации об иероглифах, радикалах, порядке черт и классификации.
    /// </summary>
    public interface ICharacterService
    {
        #region Основные операции с иероглифами

        /// <summary>
        /// Получает информацию об иероглифе по идентификатору карточки.
        /// </summary>
        Task<Card> GetCharacterByCardIdAsync(int cardId);

        /// <summary>
        /// Получает информацию об иероглифе по самому символу.
        /// </summary>
        Task<Card?> GetCharacterBySymbolAsync(string character);

        /// <summary>
        /// Ищет иероглифы по тексту (символу, пиньиню или переводу).
        /// </summary>
        Task<List<Card>> SearchCharactersAsync(string searchText);

        /// <summary>
        /// Получает иероглифы по уровню HSK.
        /// </summary>
        Task<List<Card>> GetCharactersByHskLevelAsync(int hskLevel);

        /// <summary>
        /// Получает иероглифы по радикалу.
        /// </summary>
        Task<List<Card>> GetCharactersByRadicalAsync(string radical);

        /// <summary>
        /// Получает иероглифы по типу иероглифа.
        /// </summary>
        Task<List<Card>> GetCharactersByTypeAsync(string characterType);

        /// <summary>
        /// Получает случайные иероглифы для изучения.
        /// </summary>
        Task<List<Card>> GetRandomCharactersAsync(int count);

        /// <summary>
        /// Получает рекомендуемые иероглифы для изучения на основе прогресса пользователя.
        /// </summary>
        Task<List<Card>> GetRecommendedCharactersAsync(int limit = 10);

        /// <summary>
        /// Получает количество иероглифов в базе данных.
        /// </summary>
        Task<int> GetCharacterCountAsync();

        /// <summary>
        /// Получает количество иероглифов по уровню HSK.
        /// </summary>
        Task<int> GetCharacterCountByHskLevelAsync(int hskLevel);

        #endregion

        #region Операции с радикалами и компонентами

        /// <summary>
        /// Получает все радикалы (ключевые компоненты иероглифов).
        /// </summary>
        Task<List<string>> GetAllRadicalsAsync();

        /// <summary>
        /// Получает радикалы по категории или группе.
        /// </summary>
        Task<List<string>> GetRadicalsByCategoryAsync(string category);

        /// <summary>
        /// Получает информацию о радикале (название, значение, примеры иероглифов).
        /// </summary>
        Task<RadicalInfo> GetRadicalInfoAsync(string radical);

        /// <summary>
        /// Получает компоненты, из которых состоит иероглиф.
        /// </summary>
        Task<List<string>> GetCharacterComponentsAsync(string character);

        /// <summary>
        /// Получает иероглифы, содержащие указанный компонент.
        /// </summary>
        Task<List<Card>> GetCharactersByComponentAsync(string component);

        /// <summary>
        /// Анализирует структуру иероглифа (расположение компонентов).
        /// </summary>
        Task<CharacterStructure> AnalyzeCharacterStructureAsync(string character);

        #endregion

        #region Операции с порядком черт (Stroke Order)

        /// <summary>
        /// Получает данные о порядке черт для иероглифа.
        /// </summary>
        Task<StrokeOrderData> GetStrokeOrderAsync(string character);

        /// <summary>
        /// Получает количество черт в иероглифе.
        /// </summary>
        Task<int> GetStrokeCountAsync(string character);

        /// <summary>
        /// Получает анимацию порядка черт (URL или данные для отображения).
        /// </summary>
        Task<StrokeAnimation> GetStrokeAnimationAsync(string character);

        /// <summary>
        /// Получает иероглифы с определенным количеством черт.
        /// </summary>
        Task<List<Card>> GetCharactersByStrokeCountAsync(int strokeCount, int? minCount = null, int? maxCount = null);

        /// <summary>
        /// Проверяет правильность порядка черт (для режима практики написания).
        /// </summary>
        Task<bool> ValidateStrokeOrderAsync(string character, List<Stroke> userStrokes);

        /// <summary>
        /// Получает рекомендации по написанию иероглифа (типичные ошибки, советы).
        /// </summary>
        Task<WritingTips> GetWritingTipsAsync(string character);

        #endregion

        #region Классификация иероглифов

        /// <summary>
        /// Получает все типы иероглифов согласно традиционной классификации.
        /// </summary>
        Task<List<CharacterType>> GetAllCharacterTypesAsync();

        /// <summary>
        /// Получает тип иероглифа по символу.
        /// </summary>
        Task<CharacterType> GetCharacterTypeBySymbolAsync(string character);

        /// <summary>
        /// Получает подробное объяснение типа иероглифа.
        /// </summary>
        Task<CharacterTypeExplanation> GetCharacterTypeExplanationAsync(string characterTypeName);

        /// <summary>
        /// Получает примеры иероглифов определенного типа.
        /// </summary>
        Task<List<Card>> GetExamplesByCharacterTypeAsync(string characterType);

        /// <summary>
        /// Определяет тип иероглифа на основе его структуры и компонентов.
        /// </summary>
        Task<string> ClassifyCharacterAsync(string character);

        /// <summary>
        /// Получает статистику по типам иероглифов (сколько иероглифов каждого типа).
        /// </summary>
        Task<Dictionary<string, int>> GetCharacterTypeStatisticsAsync();

        #endregion

        #region Статистика и аналитика

        /// <summary>
        /// Получает статистику изучения иероглифов пользователем.
        /// </summary>
        Task<CharacterStudyStats> GetCharacterStudyStatsAsync();

        /// <summary>
        /// Получает прогресс изучения по уровням HSK.
        /// </summary>
        Task<List<HskLevelProgress>> GetHskLevelProgressAsync();

        /// <summary>
        /// Получает прогресс изучения по радикалам.
        /// </summary>
        Task<List<RadicalProgress>> GetRadicalProgressAsync();

        /// <summary>
        /// Получает прогресс изучения по типам иероглифов.
        /// </summary>
        Task<List<CharacterTypeProgress>> GetCharacterTypeProgressAsync();

        /// <summary>
        /// Получает самые сложные иероглифы для пользователя (на основе статистики SRS).
        /// </summary>
        Task<List<Card>> GetMostDifficultCharactersAsync(int limit = 10);

        /// <summary>
        /// Получает самые легкие иероглифы для пользователя (на основе статистики SRS).
        /// </summary>
        Task<List<Card>> GetEasiestCharactersAsync(int limit = 10);

        /// <summary>
        /// Получает иероглифы, которые давно не повторялись.
        /// </summary>
        Task<List<Card>> GetLongUnreviewedCharactersAsync(int limit = 10);

        #endregion

        #region Утилиты и вспомогательные методы

        /// <summary>
        /// Конвертирует упрощенный иероглиф в традиционный.
        /// </summary>
        Task<string> ConvertToTraditionalAsync(string simplifiedCharacter);

        /// <summary>
        /// Конвертирует традиционный иероглиф в упрощенный.
        /// </summary>
        Task<string> ConvertToSimplifiedAsync(string traditionalCharacter);

        /// <summary>
        /// Получает варианты произношения (полифония) для иероглифа.
        /// </summary>
        Task<List<PronunciationVariant>> GetPronunciationVariantsAsync(string character);

        /// <summary>
        /// Получает частотность иероглифа (ранк на основе корпуса текстов).
        /// </summary>
        Task<int> GetCharacterFrequencyRankAsync(string character);

        /// <summary>
        /// Получает иероглифы в порядке частотности.
        /// </summary>
        Task<List<Card>> GetCharactersByFrequencyAsync(int limit = 50);

        /// <summary>
        /// Проверяет, существует ли иероглиф в базе данных.
        /// </summary>
        Task<bool> CharacterExistsAsync(string character);

        /// <summary>
        /// Получает похожие иероглифы (по внешнему виду или компонентам).
        /// </summary>
        Task<List<Card>> GetSimilarCharactersAsync(string character, int limit = 10);

        /// <summary>
        /// Инициализирует сервис (загружает данные, кэширует и т.д.).
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Очищает кэш сервиса.
        /// </summary>
        Task ClearCacheAsync();

        #endregion
    }

    #region Вспомогательные классы

    /// <summary>
    /// Информация о радикале.
    /// </summary>
    public class RadicalInfo
    {
        public string Radical { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public int StrokeCount { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> ExampleCharacters { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Структура иероглифа.
    /// </summary>
    public class CharacterStructure
    {
        public string Character { get; set; } = string.Empty;
        public string StructureType { get; set; } = string.Empty; // Лево-право, верх-низ и т.д.
        public List<ComponentPosition> Components { get; set; } = new List<ComponentPosition>();
        public string PatternDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Позиция компонента в иероглифе.
    /// </summary>
    public class ComponentPosition
    {
        public string Component { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty; // left, right, top, bottom, enclosure и т.д.
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// Данные о порядке черт.
    /// </summary>
    public class StrokeOrderData
    {
        public string Character { get; set; } = string.Empty;
        public int TotalStrokes { get; set; }
        public List<Stroke> Strokes { get; set; } = new List<Stroke>();
        public string Rules { get; set; } = string.Empty;
        public string CommonMistakes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Черта иероглифа.
    /// </summary>
    public class Stroke
    {
        public int Number { get; set; }
        public string Type { get; set; } = string.Empty; // horizontal, vertical, dot и т.д.
        public List<Point> Points { get; set; } = new List<Point>();
        public string Direction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Точка в черте.
    /// </summary>
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// Анимация порядка черт.
    /// </summary>
    public class StrokeAnimation
    {
        public string Character { get; set; } = string.Empty;
        public string AnimationUrl { get; set; } = string.Empty;
        public string DataFormat { get; set; } = string.Empty; // SVG, GIF, JSON и т.д.
        public int FrameCount { get; set; }
        public int FrameDelay { get; set; } // Задержка между кадрами в мс
    }

    /// <summary>
    /// Советы по написанию.
    /// </summary>
    public class WritingTips
    {
        public string Character { get; set; } = string.Empty;
        public List<string> Tips { get; set; } = new List<string>();
        public List<string> CommonErrors { get; set; } = new List<string>();
        public string Mnemonic { get; set; } = string.Empty;
        public string MemoryAid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Объяснение типа иероглифа.
    /// </summary>
    public class CharacterTypeExplanation
    {
        public string TypeName { get; set; } = string.Empty;
        public string ChineseName { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string DetailedDescription { get; set; } = string.Empty;
        public string FormationPrinciple { get; set; } = string.Empty;
        public string HistoricalContext { get; set; } = string.Empty;
        public List<string> KeyCharacteristics { get; set; } = new List<string>();
        public double PercentageOfAllCharacters { get; set; }
    }

    /// <summary>
    /// Вариант произношения.
    /// </summary>
    public class PronunciationVariant
    {
        public string Pinyin { get; set; } = string.Empty;
        public string Tone { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public bool IsMainPronunciation { get; set; }
        public int Frequency { get; set; }
    }

    /// <summary>
    /// Статистика изучения иероглифов.
    /// </summary>
    public class CharacterStudyStats
    {
        public int TotalCharacters { get; set; }
        public int CharactersLearned { get; set; }
        public int CharactersInProgress { get; set; }
        public int CharactersNotStarted { get; set; }
        public double OverallProgress { get; set; }
        public int TotalStudyTimeHours { get; set; }
        public double AverageAccuracy { get; set; }
        public DateTime LastStudyDate { get; set; }
        public int StudyStreakDays { get; set; }
    }

    /// <summary>
    /// Прогресс по уровню HSK.
    /// </summary>
    public class HskLevelProgress
    {
        public int HskLevel { get; set; }
        public int TotalCharacters { get; set; }
        public int CharactersLearned { get; set; }
        public double ProgressPercentage { get; set; }
        public double AverageAccuracy { get; set; }
        public double AverageEFactor { get; set; }
    }

    /// <summary>
    /// Прогресс по радикалам.
    /// </summary>
    public class RadicalProgress
    {
        public string Radical { get; set; } = string.Empty;
        public int TotalCharacters { get; set; }
        public int CharactersLearned { get; set; }
        public double ProgressPercentage { get; set; }
        public double AverageDifficulty { get; set; }
    }

    /// <summary>
    /// Прогресс по типам иероглифов.
    /// </summary>
    public class CharacterTypeProgress
    {
        public string CharacterType { get; set; } = string.Empty;
        public int TotalCharacters { get; set; }
        public int CharactersLearned { get; set; }
        public double ProgressPercentage { get; set; }
        public double AverageDifficulty { get; set; }
        public double AverageStudyTime { get; set; }
    }

    #endregion
}
