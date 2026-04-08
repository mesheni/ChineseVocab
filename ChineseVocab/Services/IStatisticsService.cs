using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Интерфейс для сервиса статистики и аналитики.
    /// Предоставляет методы для сбора, анализа и отображения статистики изучения китайского языка.
    /// </summary>
    public interface IStatisticsService
    {
        #region Основная статистика

        /// <summary>
        /// Получает общую сводку статистики изучения.
        /// </summary>
        Task<OverallStatistics> GetOverallStatisticsAsync();

        /// <summary>
        /// Получает статистику за сегодня.
        /// </summary>
        Task<DailyStatistics> GetTodayStatisticsAsync();

        /// <summary>
        /// Получает статистику за указанный день.
        /// </summary>
        Task<DailyStatistics> GetStatisticsForDateAsync(DateTime date);

        /// <summary>
        /// Получает статистику за период.
        /// </summary>
        Task<PeriodStatistics> GetStatisticsForPeriodAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получает текущую серию дней изучения подряд.
        /// </summary>
        Task<int> GetCurrentStudyStreakAsync();

        /// <summary>
        /// Получает максимальную серию дней изучения подряд.
        /// </summary>
        Task<int> GetMaxStudyStreakAsync();

        /// <summary>
        /// Получает общее время, потраченное на изучение.
        /// </summary>
        Task<TimeSpan> GetTotalStudyTimeAsync();

        /// <summary>
        /// Получает среднее время изучения в день.
        /// </summary>
        Task<TimeSpan> GetAverageDailyStudyTimeAsync();

        #endregion

        #region Статистика по времени

        /// <summary>
        /// Получает ежедневную статистику за указанный период.
        /// </summary>
        Task<List<DailyStatistics>> GetDailyStatisticsForPeriodAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получает еженедельную статистику за указанный период.
        /// </summary>
        Task<List<WeeklyStatistics>> GetWeeklyStatisticsForPeriodAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получает месячную статистику за указанный период.
        /// </summary>
        Task<List<MonthlyStatistics>> GetMonthlyStatisticsForPeriodAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получает статистику по времени суток (когда пользователь обычно изучает).
        /// </summary>
        Task<TimeOfDayStatistics> GetTimeOfDayStatisticsAsync();

        /// <summary>
        /// Получает статистику по дням недели.
        /// </summary>
        Task<DayOfWeekStatistics> GetDayOfWeekStatisticsAsync();

        #endregion

        #region Статистика по карточкам и изучению

        /// <summary>
        /// Получает статистику по карточкам (новые, изученные, для повторения).
        /// </summary>
        Task<CardStatistics> GetCardStatisticsAsync();

        /// <summary>
        /// Получает прогресс изучения карточек по уровням HSK.
        /// </summary>
        Task<List<HskLevelStatistics>> GetHskLevelStatisticsAsync();

        /// <summary>
        /// Получает статистику успеваемости по карточкам.
        /// </summary>
        Task<PerformanceStatistics> GetPerformanceStatisticsAsync();

        /// <summary>
        /// Получает статистику по типам иероглифов.
        /// </summary>
        Task<List<CharacterTypeStatistics>> GetCharacterTypeStatisticsAsync();

        /// <summary>
        /// Получает статистику по радикалам.
        /// </summary>
        Task<List<RadicalStatistics>> GetRadicalStatisticsAsync();

        /// <summary>
        /// Получает статистику по сложности карточек.
        /// </summary>
        Task<DifficultyStatistics> GetDifficultyStatisticsAsync();

        /// <summary>
        /// Получает самые сложные карточки для пользователя.
        /// </summary>
        Task<List<Card>> GetMostDifficultCardsAsync(int limit = 10);

        /// <summary>
        /// Получает самые легкие карточки для пользователя.
        /// </summary>
        Task<List<Card>> GetEasiestCardsAsync(int limit = 10);

        #endregion

        #region Статистика по режимам обучения

        /// <summary>
        /// Получает статистику по режиму изучения карточек.
        /// </summary>
        Task<StudyModeStatistics> GetStudyModeStatisticsAsync();

        /// <summary>
        /// Получает статистику по режиму диктанта.
        /// </summary>
        Task<DictationModeStatistics> GetDictationModeStatisticsAsync();

        /// <summary>
        /// Получает статистику по режиму библиотеки иероглифов.
        /// </summary>
        Task<CharacterLibraryStatistics> GetCharacterLibraryStatisticsAsync();

        /// <summary>
        /// Получает статистику по режиму примеров предложений.
        /// </summary>
        Task<SentenceModeStatistics> GetSentenceModeStatisticsAsync();

        #endregion

        #region Аналитика и рекомендации

        /// <summary>
        /// Получает рекомендации по улучшению процесса изучения.
        /// </summary>
        Task<List<StudyRecommendation>> GetStudyRecommendationsAsync();

        /// <summary>
        /// Получает прогноз прогресса на основе текущей статистики.
        /// </summary>
        Task<ProgressForecast> GetProgressForecastAsync();

        /// <summary>
        /// Получает анализ слабых мест пользователя.
        /// </summary>
        Task<WeaknessAnalysis> GetWeaknessAnalysisAsync();

        /// <summary>
        /// Получает цели и достижения пользователя.
        /// </summary>
        Task<AchievementStatistics> GetAchievementStatisticsAsync();

        /// <summary>
        /// Получает сравнение с другими пользователями (анонимное).
        /// </summary>
        Task<ComparisonStatistics> GetComparisonStatisticsAsync();

        #endregion

        #region Экспорт и управление

        /// <summary>
        /// Экспортирует статистику в формате JSON.
        /// </summary>
        Task<string> ExportStatisticsToJsonAsync();

        /// <summary>
        /// Экспортирует статистику в формате CSV.
        /// </summary>
        Task<string> ExportStatisticsToCsvAsync();

        /// <summary>
        /// Сбрасывает статистику (только для тестирования).
        /// </summary>
        Task ResetStatisticsAsync();

        /// <summary>
        /// Инициализирует сервис статистики.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Очищает кэш статистики.
        /// </summary>
        Task ClearCacheAsync();

        #endregion
    }

    #region Вспомогательные классы

    /// <summary>
    /// Общая статистика изучения.
    /// </summary>
    public class OverallStatistics
    {
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public int CardsInProgress { get; set; }
        public int CardsNotStarted { get; set; }
        public int TotalStudyDays { get; set; }
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }
        public TimeSpan TotalStudyTime { get; set; }
        public double OverallProgress { get; set; }
        public double AverageAccuracy { get; set; }
        public DateTime FirstStudyDate { get; set; }
        public DateTime LastStudyDate { get; set; }
        public int TotalStudySessions { get; set; }
        public int TotalCardsReviewed { get; set; }
        public int TotalNewCardsLearned { get; set; }
    }

    /// <summary>
    /// Ежедневная статистика.
    /// </summary>
    public class DailyStatistics
    {
        public DateTime Date { get; set; }
        public int CardsStudied { get; set; }
        public int CardsReviewed { get; set; }
        public int NewCardsLearned { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public double AccuracyRate => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
        public TimeSpan StudyTime { get; set; }
        public int StudySessions { get; set; }
        public double AverageEFactor { get; set; }
        public int Streak { get; set; }
        public bool StudyGoalAchieved { get; set; }
    }

    /// <summary>
    /// Статистика за период.
    /// </summary>
    public class PeriodStatistics
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public int StudyDays { get; set; }
        public double StudyConsistency => TotalDays > 0 ? (double)StudyDays / TotalDays * 100 : 0;
        public int TotalCardsStudied { get; set; }
        public int TotalCardsReviewed { get; set; }
        public int TotalNewCardsLearned { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public double AverageAccuracy => TotalAnswers > 0 ? (double)TotalCorrectAnswers / TotalAnswers * 100 : 0;
        public TimeSpan TotalStudyTime { get; set; }
        public TimeSpan AverageDailyStudyTime => StudyDays > 0 ? TimeSpan.FromTicks(TotalStudyTime.Ticks / StudyDays) : TimeSpan.Zero;
        public int MaxStreak { get; set; }
        public double AverageEFactor { get; set; }
    }

    /// <summary>
    /// Еженедельная статистика.
    /// </summary>
    public class WeeklyStatistics
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int WeekNumber { get; set; }
        public int StudyDays { get; set; }
        public int CardsStudied { get; set; }
        public int CardsReviewed { get; set; }
        public int NewCardsLearned { get; set; }
        public double AccuracyRate { get; set; }
        public TimeSpan StudyTime { get; set; }
        public double ProgressChange { get; set; }
    }

    /// <summary>
    /// Ежемесячная статистика.
    /// </summary>
    public class MonthlyStatistics
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int StudyDays { get; set; }
        public int CardsStudied { get; set; }
        public int CardsReviewed { get; set; }
        public int NewCardsLearned { get; set; }
        public double AccuracyRate { get; set; }
        public TimeSpan StudyTime { get; set; }
        public double ProgressChange { get; set; }
        public int BestStreak { get; set; }
    }

    /// <summary>
    /// Статистика по времени суток.
    /// </summary>
    public class TimeOfDayStatistics
    {
        public int MorningSessions { get; set; } // 6:00-12:00
        public int AfternoonSessions { get; set; } // 12:00-18:00
        public int EveningSessions { get; set; } // 18:00-24:00
        public int NightSessions { get; set; } // 0:00-6:00
        public double MorningAccuracy { get; set; }
        public double AfternoonAccuracy { get; set; }
        public double EveningAccuracy { get; set; }
        public double NightAccuracy { get; set; }
        public TimeSpan PreferredStudyTime { get; set; }
    }

    /// <summary>
    /// Статистика по дням недели.
    /// </summary>
    public class DayOfWeekStatistics
    {
        public double MondayAccuracy { get; set; }
        public double TuesdayAccuracy { get; set; }
        public double WednesdayAccuracy { get; set; }
        public double ThursdayAccuracy { get; set; }
        public double FridayAccuracy { get; set; }
        public double SaturdayAccuracy { get; set; }
        public double SundayAccuracy { get; set; }
        public int MondaySessions { get; set; }
        public int TuesdaySessions { get; set; }
        public int WednesdaySessions { get; set; }
        public int ThursdaySessions { get; set; }
        public int FridaySessions { get; set; }
        public int SaturdaySessions { get; set; }
        public int SundaySessions { get; set; }
        public string MostProductiveDay { get; set; } = string.Empty;
        public string LeastProductiveDay { get; set; } = string.Empty;
    }

    /// <summary>
    /// Статистика по карточкам.
    /// </summary>
    public class CardStatistics
    {
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public int CardsToReview { get; set; }
        public int CardsDueToday { get; set; }
        public int CardsDueTomorrow { get; set; }
        public int NewCardsAvailable { get; set; }
        public double LearningProgress => TotalCards > 0 ? (double)CardsLearned / TotalCards * 100 : 0;
        public double ReviewLoad => TotalCards > 0 ? (double)CardsToReview / TotalCards * 100 : 0;
        public double AverageEFactor { get; set; }
        public double AverageEaseScore { get; set; }
    }

    /// <summary>
    /// Статистика по уровням HSK.
    /// </summary>
    public class HskLevelStatistics
    {
        public int HskLevel { get; set; }
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public int CardsInProgress { get; set; }
        public int CardsNotStarted { get; set; }
        public double LearningProgress => TotalCards > 0 ? (double)CardsLearned / TotalCards * 100 : 0;
        public double AverageAccuracy { get; set; }
        public double AverageEFactor { get; set; }
        public TimeSpan AverageStudyTime { get; set; }
    }

    /// <summary>
    /// Статистика успеваемости.
    /// </summary>
    public class PerformanceStatistics
    {
        public double OverallAccuracy { get; set; }
        public double RecentAccuracy { get; set; } // За последние 7 дней
        public double TrendAccuracy { get; set; } // Тренд точности
        public int BestAccuracyDay { get; set; }
        public double BestAccuracyRate { get; set; }
        public DateTime BestAccuracyDate { get; set; }
        public int WorstAccuracyDay { get; set; }
        public double WorstAccuracyRate { get; set; }
        public DateTime WorstAccuracyDate { get; set; }
        public double ConsistencyScore { get; set; }
        public double ImprovementRate { get; set; }
    }

    /// <summary>
    /// Статистика по типам иероглифов.
    /// </summary>
    public class CharacterTypeStatistics
    {
        public string CharacterType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public double LearningProgress => TotalCards > 0 ? (double)CardsLearned / TotalCards * 100 : 0;
        public double AverageAccuracy { get; set; }
        public double AverageDifficulty { get; set; }
        public TimeSpan AverageStudyTime { get; set; }
    }

    /// <summary>
    /// Статистика по радикалам.
    /// </summary>
    public class RadicalStatistics
    {
        public string Radical { get; set; } = string.Empty;
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public double LearningProgress => TotalCards > 0 ? (double)CardsLearned / TotalCards * 100 : 0;
        public double AverageAccuracy { get; set; }
        public double AverageDifficulty { get; set; }
        public int RelatedCardsCount { get; set; }
    }

    /// <summary>
    /// Статистика по сложности.
    /// </summary>
    public class DifficultyStatistics
    {
        public int EasyCards { get; set; }
        public int MediumCards { get; set; }
        public int HardCards { get; set; }
        public int VeryHardCards { get; set; }
        public double EasyCardsAccuracy { get; set; }
        public double MediumCardsAccuracy { get; set; }
        public double HardCardsAccuracy { get; set; }
        public double VeryHardCardsAccuracy { get; set; }
        public double AverageDifficultyScore { get; set; }
        public string MostDifficultCategory { get; set; } = string.Empty;
    }

    /// <summary>
    /// Статистика по режиму изучения карточек.
    /// </summary>
    public class StudyModeStatistics
    {
        public int TotalSessions { get; set; }
        public int TotalCardsStudied { get; set; }
        public double AverageAccuracy { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan AverageSessionTime { get; set; }
        public int BestSessionCards { get; set; }
        public DateTime BestSessionDate { get; set; }
        public double BestSessionAccuracy { get; set; }
        public int LongestSessionMinutes { get; set; }
        public DateTime LongestSessionDate { get; set; }
    }

    /// <summary>
    /// Статистика по режиму диктанта.
    /// </summary>
    public class DictationModeStatistics
    {
        public int TotalSessions { get; set; }
        public int TotalCharactersWritten { get; set; }
        public int TotalCorrectCharacters { get; set; }
        public double AccuracyRate => TotalCharactersWritten > 0 ? (double)TotalCorrectCharacters / TotalCharactersWritten * 100 : 0;
        public TimeSpan TotalTime { get; set; }
        public int BestSessionScore { get; set; }
        public DateTime BestSessionDate { get; set; }
        public string MostPracticedCharacter { get; set; } = string.Empty;
        public int MostPracticedCharacterCount { get; set; }
    }

    /// <summary>
    /// Статистика по режиму библиотеки иероглифов.
    /// </summary>
    public class CharacterLibraryStatistics
    {
        public int TotalViews { get; set; }
        public int UniqueCharactersViewed { get; set; }
        public int SearchesPerformed { get; set; }
        public string MostSearchedTerm { get; set; } = string.Empty;
        public int MostSearchedTermCount { get; set; }
        public string MostViewedCharacter { get; set; } = string.Empty;
        public int MostViewedCharacterCount { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
    }

    /// <summary>
    /// Статистика по режиму примеров предложений.
    /// </summary>
    public class SentenceModeStatistics
    {
        public int TotalSentencesViewed { get; set; }
        public int TotalExercisesCompleted { get; set; }
        public double ExerciseAccuracy { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
        public string MostViewedSentence { get; set; } = string.Empty;
        public int MostViewedSentenceCount { get; set; }
        public int SentencesTranslated { get; set; }
        public double TranslationAccuracy { get; set; }
    }

    /// <summary>
    /// Рекомендация по изучению.
    /// </summary>
    public class StudyRecommendation
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // accuracy, consistency, speed, etc.
        public int Priority { get; set; } // 1-5, где 5 - самый высокий приоритет
        public string Action { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ExpectedBenefit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Прогноз прогресса.
    /// </summary>
    public class ProgressForecast
    {
        public DateTime ForecastDate { get; set; }
        public int EstimatedCardsLearned { get; set; }
        public double EstimatedProgress { get; set; }
        public int DaysToCompleteCurrentLevel { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public double ConfidenceLevel { get; set; } // 0-1
        public List<ForecastScenario> Scenarios { get; set; } = new List<ForecastScenario>();
    }

    /// <summary>
    /// Сценарий прогноза.
    /// </summary>
    public class ForecastScenario
    {
        public string Scenario { get; set; } = string.Empty; // optimistic, realistic, pessimistic
        public int EstimatedCardsLearned { get; set; }
        public double EstimatedProgress { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
    }

    /// <summary>
    /// Анализ слабых мест.
    /// </summary>
    public class WeaknessAnalysis
    {
        public List<WeaknessArea> WeaknessAreas { get; set; } = new List<WeaknessArea>();
        public string PrimaryWeakness { get; set; } = string.Empty;
        public double OverallWeaknessScore { get; set; } // 0-100, где 100 - много слабых мест
        public List<string> ImprovementSuggestions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Область слабости.
    /// </summary>
    public class WeaknessArea
    {
        public string Area { get; set; } = string.Empty; // hsk level, character type, radical, etc.
        public string Name { get; set; } = string.Empty;
        public double WeaknessScore { get; set; } // 0-100, где 100 - самая слабая область
        public double Accuracy { get; set; }
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public List<string> SpecificProblems { get; set; } = new List<string>();
    }

    /// <summary>
    /// Статистика достижений.
    /// </summary>
    public class AchievementStatistics
    {
        public int TotalAchievements { get; set; }
        public int AchievementsUnlocked { get; set; }
        public double AchievementProgress => TotalAchievements > 0 ? (double)AchievementsUnlocked / TotalAchievements * 100 : 0;
        public List<Achievement> RecentAchievements { get; set; } = new List<Achievement>();
        public List<Achievement> NextAchievements { get; set; } = new List<Achievement>();
        public int TotalPoints { get; set; }
        public int Level { get; set; }
        public int PointsToNextLevel { get; set; }
    }

    /// <summary>
    /// Достижение.
    /// </summary>
    public class Achievement
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public DateTime UnlockDate { get; set; }
        public int Points { get; set; }
        public string Icon { get; set; } = string.Empty;
        public double Progress { get; set; } // 0-100
    }

    /// <summary>
    /// Сравнительная статистика.
    /// </summary>
    public class ComparisonStatistics
    {
        public double PercentileAccuracy { get; set; } // Процентиль по точности
        public double PercentileConsistency { get; set; } // Процентиль по постоянству
        public double PercentileProgress { get; set; } // Процентиль по прогрессу
        public int RankTotalUsers { get; set; }
        public int UserRank { get; set; }
        public string ComparisonGroup { get; set; } = string.Empty; // beginner, intermediate, advanced
        public List<ComparisonMetric> Metrics { get; set; } = new List<ComparisonMetric>();
    }

    /// <summary>
    /// Сравнительная метрика.
    /// </summary>
    public class ComparisonMetric
    {
        public string Metric { get; set; } = string.Empty;
        public double UserValue { get; set; }
        public double GroupAverage { get; set; }
        public double GroupTop10Percent { get; set; }
        public ComparisonStatus Status { get; set; } // below_average, average, above_average, top_performer
    }

    /// <summary>
    /// Статус сравнения.
    /// </summary>
    public enum ComparisonStatus
    {
        BelowAverage,
        Average,
        AboveAverage,
        TopPerformer
    }

    #endregion
}
