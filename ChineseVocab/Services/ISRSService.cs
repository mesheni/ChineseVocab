using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Интерфейс для сервиса системы интервальных повторений (Spaced Repetition System).
    /// Реализует алгоритм SM-2 для оптимизации процесса запоминания.
    /// </summary>
    public interface ISRSService
    {
        #region Основные операции SRS

        /// <summary>
        /// Обрабатывает оценку карточки пользователем по алгоритму SM-2.
        /// </summary>
        /// <param name="cardId">Идентификатор карточки</param>
        /// <param name="quality">Оценка качества ответа (0-5):
        /// 0 - полный провал
        /// 1 - неправильный ответ
        /// 2 - почти правильно
        /// 3 - правильно с затруднениями
        /// 4 - правильно легко
        /// 5 - правильно мгновенно
        /// </param>
        /// <returns>Обновленная статистика SRS</returns>
        Task<SRSStat> ProcessCardReviewAsync(int cardId, int quality);

        /// <summary>
        /// Получает карточки, которые нужно повторить (дата следующего повторения наступила).
        /// </summary>
        /// <param name="limit">Ограничение количества возвращаемых карточек (0 для без ограничений)</param>
        /// <returns>Список карточек для повторения</returns>
        Task<List<Card>> GetDueCardsAsync(int limit = 0);

        /// <summary>
        /// Получает карточки из определенной колоды, которые нужно повторить.
        /// </summary>
        /// <param name="deckId">Идентификатор колоды</param>
        /// <param name="limit">Ограничение количества возвращаемых карточек</param>
        /// <returns>Список карточек для повторения</returns>
        Task<List<Card>> GetDueCardsByDeckAsync(int deckId, int limit = 0);

        /// <summary>
        /// Получает карточки для изучения сегодня (новые карточки + карточки для повторения).
        /// </summary>
        /// <param name="newCardsLimit">Максимальное количество новых карточек для изучения</param>
        /// <param name="reviewCardsLimit">Максимальное количество карточек для повторения</param>
        /// <returns>Объединенный список карточек для изучения</returns>
        Task<List<Card>> GetStudySessionCardsAsync(int newCardsLimit = 10, int reviewCardsLimit = 20);

        /// <summary>
        /// Получает статистику SRS для карточки.
        /// </summary>
        /// <param name="cardId">Идентификатор карточки</param>
        /// <returns>Статистика SRS или null, если не найдена</returns>
        Task<SRSStat> GetSRSStatForCardAsync(int cardId);

        /// <summary>
        /// Создает или обновляет статистику SRS для карточки.
        /// </summary>
        /// <param name="stat">Статистика SRS</param>
        Task SaveSRSStatAsync(SRSStat stat);

        /// <summary>
        /// Сбрасывает прогресс изучения карточки.
        /// </summary>
        /// <param name="cardId">Идентификатор карточки</param>
        Task ResetCardProgressAsync(int cardId);

        /// <summary>
        /// Помечает карточку как выученную.
        /// </summary>
        /// <param name="cardId">Идентификатор карточки</param>
        Task MarkCardAsLearnedAsync(int cardId);

        #endregion

        #region Статистика и аналитика

        /// <summary>
        /// Получает сводку по изучению (количество карточек для повторения, изученных и т.д.).
        /// </summary>
        Task<SRSStudySummary> GetStudySummaryAsync();

        /// <summary>
        /// Получает количество карточек для повторения на сегодня.
        /// </summary>
        Task<int> GetDueCardsCountAsync();

        /// <summary>
        /// Получает количество карточек для повторения на завтра.
        /// </summary>
        Task<int> GetDueCardsCountForTomorrowAsync();

        /// <summary>
        /// Получает количество карточек для повторения на определенную дату.
        /// </summary>
        Task<int> GetDueCardsCountForDateAsync(DateTime date);

        /// <summary>
        /// Получает количество новых карточек, доступных для изучения.
        /// </summary>
        Task<int> GetNewCardsCountAsync();

        /// <summary>
        /// Получает количество изученных карточек.
        /// </summary>
        Task<int> GetLearnedCardsCountAsync();

        /// <summary>
        /// Получает текущую серию правильных ответов подряд.
        /// </summary>
        Task<int> GetCurrentStreakAsync();

        /// <summary>
        /// Получает максимальную серию правильных ответов подряд.
        /// </summary>
        Task<int> GetMaxStreakAsync();

        /// <summary>
        /// Получает средний фактор легкости (E-Factor) для всех карточек.
        /// </summary>
        Task<double> GetAverageEFactorAsync();

        /// <summary>
        /// Получает среднюю оценку (Ease Score) для всех карточек.
        /// </summary>
        Task<double> GetAverageEaseScoreAsync();

        /// <summary>
        /// Получает статистику изучения по дням за указанный период.
        /// </summary>
        Task<List<SRSDailyStudyStats>> GetDailyStudyStatsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получает статистику по уровням HSK.
        /// </summary>
        Task<List<HskLevelSRSStats>> GetHskLevelStatsAsync();

        #endregion

        #region Настройки SRS

        /// <summary>
        /// Получает настройки SRS.
        /// </summary>
        Task<SRSSettings> GetSettingsAsync();

        /// <summary>
        /// Сохраняет настройки SRS.
        /// </summary>
        Task SaveSettingsAsync(SRSSettings settings);

        /// <summary>
        /// Сбрасывает настройки SRS к значениям по умолчанию.
        /// </summary>
        Task ResetSettingsToDefaultAsync();

        /// <summary>
        /// Получает лимит новых карточек в день.
        /// </summary>
        Task<int> GetDailyNewCardsLimitAsync();

        /// <summary>
        /// Устанавливает лимит новых карточек в день.
        /// </summary>
        Task SetDailyNewCardsLimitAsync(int limit);

        /// <summary>
        /// Получает лимит карточек для повторения в день.
        /// </summary>
        Task<int> GetDailyReviewCardsLimitAsync();

        /// <summary>
        /// Устанавливает лимит карточек для повторения в день.
        /// </summary>
        Task SetDailyReviewCardsLimitAsync(int limit);

        /// <summary>
        /// Получает минимальный интервал для карточек (в днях).
        /// </summary>
        Task<int> GetMinimumIntervalAsync();

        /// <summary>
        /// Устанавливает минимальный интервал для карточек (в днях).
        /// </summary>
        Task SetMinimumIntervalAsync(int days);

        /// <summary>
        /// Получает максимальный интервал для карточек (в днях).
        /// </summary>
        Task<int> GetMaximumIntervalAsync();

        /// <summary>
        /// Устанавливает максимальный интервал для карточек (в днях).
        /// </summary>
        Task SetMaximumIntervalAsync(int days);

        #endregion

        #region Утилиты и вспомогательные методы

        /// <summary>
        /// Рассчитывает следующую дату повторения на основе текущей статистики и оценки.
        /// </summary>
        Task<DateTime> CalculateNextReviewDateAsync(SRSStat stat, int quality);

        /// <summary>
        /// Рассчитывает новый интервал повторения по алгоритму SM-2.
        /// </summary>
        Task<int> CalculateNextIntervalAsync(SRSStat stat, int quality);

        /// <summary>
        /// Рассчитывает новый фактор легкости (E-Factor) по алгоритму SM-2.
        /// </summary>
        Task<double> CalculateNewEFactorAsync(SRSStat stat, int quality);

        /// <summary>
        /// Проверяет, действительна ли оценка качества (0-5).
        /// </summary>
        bool IsValidQualityScore(int quality);

        /// <summary>
        /// Конвертирует оценку качества в текстовое описание.
        /// </summary>
        string QualityScoreToDescription(int quality);

        /// <summary>
        /// Инициализирует сервис (загружает настройки, подготавливает данные).
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Очищает кэш и временные данные.
        /// </summary>
        Task ClearCacheAsync();

        #endregion
    }

    /// <summary>
    /// Сводка по изучению с использованием SRS.
    /// </summary>
    public class SRSStudySummary
    {
        /// <summary>
        /// Общее количество карточек.
        /// </summary>
        public int TotalCards { get; set; }

        /// <summary>
        /// Количество карточек для повторения сегодня.
        /// </summary>
        public int DueCardsToday { get; set; }

        /// <summary>
        /// Количество карточек для повторения завтра.
        /// </summary>
        public int DueCardsTomorrow { get; set; }

        /// <summary>
        /// Количество новых карточек, доступных для изучения.
        /// </summary>
        public int NewCardsAvailable { get; set; }

        /// <summary>
        /// Количество изученных карточек (интервал больше 21 дня).
        /// </summary>
        public int LearnedCards { get; set; }

        /// <summary>
        /// Средний фактор легкости (E-Factor).
        /// </summary>
        public double AverageEFactor { get; set; }

        /// <summary>
        /// Средняя оценка (Ease Score).
        /// </summary>
        public double AverageEaseScore { get; set; }

        /// <summary>
        /// Текущая серия правильных ответов подряд.
        /// </summary>
        public int CurrentStreak { get; set; }

        /// <summary>
        /// Максимальная серия правильных ответов подряд.
        /// </summary>
        public int MaxStreak { get; set; }

        /// <summary>
        /// Процент правильных ответов.
        /// </summary>
        public double AccuracyPercentage { get; set; }

        /// <summary>
        /// Общее время изучения (в часах).
        /// </summary>
        public double TotalStudyHours { get; set; }

        /// <summary>
        /// Среднее время на карточку (в секундах).
        /// </summary>
        public double AverageTimePerCard { get; set; }

        /// <summary>
        /// Дата последнего изучения.
        /// </summary>
        public DateTime LastStudyDate { get; set; }

        /// <summary>
        /// Количество дней подряд с изучением.
        /// </summary>
        public int StudyStreakDays { get; set; }
    }

    /// <summary>
    /// Статистика изучения по дням.
    /// </summary>
    public class SRSDailyStudyStats
    {
        public DateTime Date { get; set; }
        public int CardsStudied { get; set; }
        public int CardsReviewed { get; set; }
        public int NewCardsLearned { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public double AccuracyRate => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
        public TimeSpan StudyTime { get; set; }
        public double AverageEFactor { get; set; }
        public int Streak { get; set; }
    }

    /// <summary>
    /// Статистика SRS по уровням HSK.
    /// </summary>
    public class HskLevelSRSStats
    {
        public int HskLevel { get; set; }
        public int TotalCards { get; set; }
        public int LearnedCards { get; set; }
        public double LearningProgress => TotalCards > 0 ? (double)LearnedCards / TotalCards * 100 : 0;
        public double AverageEFactor { get; set; }
        public double AverageEaseScore { get; set; }
        public int DueCards { get; set; }
        public int NewCardsAvailable { get; set; }
        public double AccuracyPercentage { get; set; }
    }

    /// <summary>
    /// Настройки системы интервальных повторений.
    /// </summary>
    public class SRSSettings
    {
        /// <summary>
        /// Лимит новых карточек в день.
        /// </summary>
        public int DailyNewCardsLimit { get; set; } = 10;

        /// <summary>
        /// Лимит карточек для повторения в день.
        /// </summary>
        public int DailyReviewCardsLimit { get; set; } = 50;

        /// <summary>
        /// Минимальный интервал между повторениями (в днях).
        /// </summary>
        public int MinimumInterval { get; set; } = 1;

        /// <summary>
        /// Максимальный интервал между повторениями (в днях).
        /// </summary>
        public int MaximumInterval { get; set; } = 365;

        /// <summary>
        /// Начальное значение фактора легкости (E-Factor).
        /// </summary>
        public double InitialEFactor { get; set; } = 2.5;

        /// <summary>
        /// Минимальное значение фактора легкости (E-Factor).
        /// </summary>
        public double MinEFactor { get; set; } = 1.3;

        /// <summary>
        /// Максимальное значение фактора легкости (E-Factor).
        /// </summary>
        public double MaxEFactor { get; set; } = 2.5;

        /// <summary>
        /// Интервал для первой успешной повторности (в днях).
        /// </summary>
        public int FirstReviewInterval { get; set; } = 1;

        /// <summary>
        /// Интервал для второй успешной повторности (в днях).
        /// </summary>
        public int SecondReviewInterval { get; set; } = 6;

        /// <summary>
        /// Порог для карточек, считающихся выученными (интервал в днях).
        /// </summary>
        public int LearnedThreshold { get; set; } = 21;

        /// <summary>
        /// Включить уведомления о повторениях.
        /// </summary>
        public bool EnableNotifications { get; set; } = true;

        /// <summary>
        /// Время уведомлений (часы).
        /// </summary>
        public int NotificationHour { get; set; } = 9;

        /// <summary>
        /// Время уведомлений (минуты).
        /// </summary>
        public int NotificationMinute { get; set; } = 0;

        /// <summary>
        /// Показывать пиньинь сразу или после ответа.
        /// </summary>
        public bool ShowPinyinImmediately { get; set; } = false;

        /// <summary>
        /// Показывать перевод сразу или после ответа.
        /// </summary>
        public bool ShowTranslationImmediately { get; set; } = false;

        /// <summary>
        /// Автоматически переходить к следующей карточке после оценки.
        /// </summary>
        public bool AutoAdvance { get; set; } = true;

        /// <summary>
        /// Задержка перед авто-переходом (в секундах).
        /// </summary>
        public int AutoAdvanceDelay { get; set; } = 2;

        /// <summary>
        /// Использовать звуковые эффекты.
        /// </summary>
        public bool UseSoundEffects { get; set; } = true;

        /// <summary>
        /// Использовать вибрацию при уведомлениях.
        /// </summary>
        public bool UseVibration { get; set; } = true;

        /// <summary>
        /// Язык интерфейса (по умолчанию русский).
        /// </summary>
        public string Language { get; set; } = "ru";

        /// <summary>
        /// Дата последнего изменения настроек.
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
