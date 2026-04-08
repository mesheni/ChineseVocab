using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Интерфейс для сервиса работы с примерами предложений на китайском языке.
    /// Предоставляет методы для получения, поиска, создания и анализа примеров предложений.
    /// </summary>
    public interface ISentenceService
    {
        #region Основные операции с предложениями

        /// <summary>
        /// Получает примеры предложений для карточки.
        /// </summary>
        Task<List<Sentence>> GetSentencesByCardIdAsync(int cardId);

        /// <summary>
        /// Получает примеры предложений для иероглифа.
        /// </summary>
        Task<List<Sentence>> GetSentencesByCharacterAsync(string character);

        /// <summary>
        /// Получает все примеры предложений.
        /// </summary>
        Task<List<Sentence>> GetAllSentencesAsync();

        /// <summary>
        /// Получает предложение по идентификатору.
        /// </summary>
        Task<Sentence> GetSentenceByIdAsync(int sentenceId);

        /// <summary>
        /// Ищет примеры предложений по тексту (китайскому, пиньиню или переводу).
        /// </summary>
        Task<List<Sentence>> SearchSentencesAsync(string searchText);

        /// <summary>
        /// Получает примеры предложений по уровню сложности.
        /// </summary>
        Task<List<Sentence>> GetSentencesByDifficultyAsync(int minDifficulty, int maxDifficulty);

        /// <summary>
        /// Получает примеры предложений по источнику.
        /// </summary>
        Task<List<Sentence>> GetSentencesBySourceAsync(string source);

        /// <summary>
        /// Получает примеры предложений по тегам.
        /// </summary>
        Task<List<Sentence>> GetSentencesByTagsAsync(string tags);

        /// <summary>
        /// Получает случайные примеры предложений для изучения.
        /// </summary>
        Task<List<Sentence>> GetRandomSentencesAsync(int count);

        /// <summary>
        /// Получает рекомендуемые примеры предложений на основе прогресса пользователя.
        /// </summary>
        Task<List<Sentence>> GetRecommendedSentencesAsync(int limit = 10);

        /// <summary>
        /// Получает количество примеров предложений в базе данных.
        /// </summary>
        Task<int> GetSentenceCountAsync();

        /// <summary>
        /// Получает количество примеров предложений для карточки.
        /// </summary>
        Task<int> GetSentenceCountByCardIdAsync(int cardId);

        /// <summary>
        /// Получает количество примеров предложений по уровню сложности.
        /// </summary>
        Task<int> GetSentenceCountByDifficultyAsync(int difficultyLevel);

        #endregion

        #region Операции с созданием и изменением предложений

        /// <summary>
        /// Создает новый пример предложения.
        /// </summary>
        Task<int> CreateSentenceAsync(Sentence sentence);

        /// <summary>
        /// Обновляет существующий пример предложения.
        /// </summary>
        Task<int> UpdateSentenceAsync(Sentence sentence);

        /// <summary>
        /// Удаляет пример предложения.
        /// </summary>
        Task<int> DeleteSentenceAsync(int sentenceId);

        /// <summary>
        /// Импортирует примеры предложений из внешнего источника.
        /// </summary>
        Task<int> ImportSentencesAsync(List<Sentence> sentences);

        /// <summary>
        /// Увеличивает счетчик показов предложения.
        /// </summary>
        Task<int> IncrementSentenceViewCountAsync(int sentenceId);

        /// <summary>
        /// Отмечает предложение как изученное пользователем.
        /// </summary>
        Task<int> MarkSentenceAsLearnedAsync(int sentenceId, int userId);

        /// <summary>
        /// Снимает отметку об изучении предложения.
        /// </summary>
        Task<int> UnmarkSentenceAsLearnedAsync(int sentenceId, int userId);

        /// <summary>
        /// Проверяет, изучено ли предложение пользователем.
        /// </summary>
        Task<bool> IsSentenceLearnedByUserAsync(int sentenceId, int userId);

        #endregion

        #region Анализ и обработка предложений

        /// <summary>
        /// Анализирует грамматическую структуру предложения.
        /// </summary>
        Task<SentenceAnalysis> AnalyzeSentenceStructureAsync(string chineseText);

        /// <summary>
        /// Получает разбор предложения по словам и грамматическим конструкциям.
        /// </summary>
        Task<SentenceBreakdown> GetSentenceBreakdownAsync(string chineseText);

        /// <summary>
        /// Получает аудио произношения предложения (если доступно).
        /// </summary>
        Task<SentenceAudio> GetSentenceAudioAsync(string chineseText);

        /// <summary>
        /// Проверяет правильность перевода предложения.
        /// </summary>
        Task<bool> ValidateTranslationAsync(string chineseText, string userTranslation);

        /// <summary>
        /// Получает альтернативные переводы предложения.
        /// </summary>
        Task<List<string>> GetAlternativeTranslationsAsync(string chineseText);

        /// <summary>
        /// Получает объяснение грамматических конструкций в предложении.
        /// </summary>
        Task<string> GetGrammarExplanationAsync(string chineseText);

        /// <summary>
        /// Получает культурные и исторические заметки о предложении.
        /// </summary>
        Task<string> GetCulturalNotesAsync(string chineseText);

        /// <summary>
        /// Получает примеры использования иероглифа в разных контекстах.
        /// </summary>
        Task<List<ContextualExample>> GetContextualExamplesAsync(string character);

        #endregion

        #region Статистика и аналитика

        /// <summary>
        /// Получает статистику изучения примеров предложений.
        /// </summary>
        Task<SentenceStudyStats> GetSentenceStudyStatsAsync();

        /// <summary>
        /// Получает статистику по уровням сложности предложений.
        /// </summary>
        Task<List<DifficultyLevelStats>> GetDifficultyLevelStatsAsync();

        /// <summary>
        /// Получает статистику по источникам предложений.
        /// </summary>
        Task<List<SourceStats>> GetSourceStatsAsync();

        /// <summary>
        /// Получает самые популярные примеры предложений.
        /// </summary>
        Task<List<Sentence>> GetMostPopularSentencesAsync(int limit = 10);

        /// <summary>
        /// Получает самые сложные примеры предложений для пользователя.
        /// </summary>
        Task<List<Sentence>> GetMostDifficultSentencesAsync(int limit = 10);

        /// <summary>
        /// Получает самые легкие примеры предложений для пользователя.
        /// </summary>
        Task<List<Sentence>> GetEasiestSentencesAsync(int limit = 10);

        /// <summary>
        /// Получает примеры предложений, которые давно не изучались.
        /// </summary>
        Task<List<Sentence>> GetLongUnreviewedSentencesAsync(int limit = 10);

        /// <summary>
        /// Получает прогресс изучения предложений по уровням HSK.
        /// </summary>
        Task<List<HskSentenceProgress>> GetHskSentenceProgressAsync();

        #endregion

        #region Утилиты и вспомогательные методы

        /// <summary>
        /// Генерирует упражнения на основе примера предложения.
        /// </summary>
        Task<List<SentenceExercise>> GenerateExercisesAsync(int sentenceId);

        /// <summary>
        /// Получает похожие примеры предложений.
        /// </summary>
        Task<List<Sentence>> GetSimilarSentencesAsync(int sentenceId, int limit = 10);

        /// <summary>
        /// Проверяет, существует ли предложение в базе данных.
        /// </summary>
        Task<bool> SentenceExistsAsync(string chineseText);

        /// <summary>
        /// Получает частотность предложения (ранк на основе корпуса текстов).
        /// </summary>
        Task<int> GetSentenceFrequencyRankAsync(string chineseText);

        /// <summary>
        /// Получает примеры предложений в порядке частотности.
        /// </summary>
        Task<List<Sentence>> GetSentencesByFrequencyAsync(int limit = 50);

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
    /// Анализ структуры предложения.
    /// </summary>
    public class SentenceAnalysis
    {
        public string ChineseText { get; set; } = string.Empty;
        public List<SentenceWord> Words { get; set; } = new List<SentenceWord>();
        public string GrammarPattern { get; set; } = string.Empty;
        public List<string> GrammarRules { get; set; } = new List<string>();
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public double ReadabilityScore { get; set; }
        public string ComplexityLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Слово в предложении.
    /// </summary>
    public class SentenceWord
    {
        public string Word { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string PartOfSpeech { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Length { get; set; }
    }

    /// <summary>
    /// Разбор предложения.
    /// </summary>
    public class SentenceBreakdown
    {
        public string ChineseText { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public List<WordBreakdown> WordBreakdowns { get; set; } = new List<WordBreakdown>();
        public List<GrammarConstruction> GrammarConstructions { get; set; } = new List<GrammarConstruction>();
        public string Explanation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Разбор слова.
    /// </summary>
    public class WordBreakdown
    {
        public string Word { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string PartOfSpeech { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string GrammarFunction { get; set; } = string.Empty;
        public List<string> AlternativeMeanings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Грамматическая конструкция.
    /// </summary>
    public class GrammarConstruction
    {
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public List<string> Examples { get; set; } = new List<string>();
        public string Difficulty { get; set; } = string.Empty;
    }

    /// <summary>
    /// Аудио предложения.
    /// </summary>
    public class SentenceAudio
    {
        public string ChineseText { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty; // MP3, WAV и т.д.
        public int DurationSeconds { get; set; }
        public string VoiceGender { get; set; } = string.Empty; // male, female
        public string Accent { get; set; } = string.Empty; // mainland, taiwan, hongkong
        public int SampleRate { get; set; }
        public int Bitrate { get; set; }
    }

    /// <summary>
    /// Контекстный пример использования.
    /// </summary>
    public class ContextualExample
    {
        public string Character { get; set; } = string.Empty;
        public string Sentence { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty; // formal, informal, literary, colloquial
        public string Source { get; set; } = string.Empty;
        public int DifficultyLevel { get; set; }
    }

    /// <summary>
    /// Статистика изучения предложений.
    /// </summary>
    public class SentenceStudyStats
    {
        public int TotalSentences { get; set; }
        public int SentencesLearned { get; set; }
        public int SentencesInProgress { get; set; }
        public int SentencesNotStarted { get; set; }
        public double OverallProgress { get; set; }
        public int TotalStudyTimeHours { get; set; }
        public double AverageAccuracy { get; set; }
        public DateTime LastStudyDate { get; set; }
        public int StudyStreakDays { get; set; }
        public int TotalViews { get; set; }
        public int TotalExercisesCompleted { get; set; }
    }

    /// <summary>
    /// Статистика по уровням сложности.
    /// </summary>
    public class DifficultyLevelStats
    {
        public int DifficultyLevel { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TotalSentences { get; set; }
        public int SentencesLearned { get; set; }
        public double ProgressPercentage { get; set; }
        public double AverageAccuracy { get; set; }
        public double AverageStudyTime { get; set; }
    }

    /// <summary>
    /// Статистика по источникам.
    /// </summary>
    public class SourceStats
    {
        public string Source { get; set; } = string.Empty;
        public int TotalSentences { get; set; }
        public int SentencesLearned { get; set; }
        public double ProgressPercentage { get; set; }
        public double AverageDifficulty { get; set; }
        public double PopularityScore { get; set; }
    }

    /// <summary>
    /// Прогресс изучения предложений по уровням HSK.
    /// </summary>
    public class HskSentenceProgress
    {
        public int HskLevel { get; set; }
        public int TotalSentences { get; set; }
        public int SentencesLearned { get; set; }
        public double ProgressPercentage { get; set; }
        public double AverageAccuracy { get; set; }
        public double AverageDifficulty { get; set; }
    }

    /// <summary>
    /// Упражнение на основе предложения.
    /// </summary>
    public class SentenceExercise
    {
        public int SentenceId { get; set; }
        public string ExerciseType { get; set; } = string.Empty; // fill-blanks, reorder, translate, etc.
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public int TimeLimitSeconds { get; set; }
        public int Points { get; set; }
    }

    #endregion
}
