using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Интерфейс сервиса планирования повторений карточек.
    /// Отвечает за управление расписанием повторений, уведомлениями и временными интервалами.
    /// </summary>
    public interface ISchedulerService
    {
        #region Основные операции планирования

        /// <summary>
        /// Получает карточки, которые нужно повторить в указанный день.
        /// </summary>
        /// <param name="date">Дата для проверки.</param>
        /// <returns>Список карточек для повторения в указанный день.</returns>
        Task<List<Card>> GetCardsForDateAsync(DateTime date);

        /// <summary>
        /// Получает карточки, которые нужно повторить сегодня.
        /// </summary>
        Task<List<Card>> GetTodayCardsAsync();

        /// <summary>
        /// Получает карточки, которые нужно повторить завтра.
        /// </summary>
        Task<List<Card>> GetTomorrowCardsAsync();

        /// <summary>
        /// Получает карточки, которые нужно повторить на следующей неделе.
        /// </summary>
        Task<List<Card>> GetNextWeekCardsAsync();

        /// <summary>
        /// Получает карточки, которые нужно повторить в течение следующих N дней.
        /// </summary>
        /// <param name="days">Количество дней вперед для проверки.</param>
        Task<List<Card>> GetCardsForNextDaysAsync(int days);

        /// <summary>
        /// Планирует следующее повторение для карточки.
        /// </summary>
        /// <param name="cardId">Идентификатор карточки.</param>
        /// <param name="nextReviewDate">Дата следующего повторения.</param>
        Task ScheduleNextReviewAsync(int cardId, DateTime nextReviewDate);

        /// <summary>
        /// Переносит повторение карточки на указанное количество дней.
        /// </summary>
        /// <param name="cardId">Идентификатор карточки.</param>
        /// <param name="daysToPostpone">Количество дней для переноса (может быть отрицательным).</param>
        Task PostponeCardAsync(int cardId, int daysToPostpone);

        #endregion

        #region Статистика и аналитика планирования

        /// <summary>
        /// Получает сводку по планированию повторений.
        /// </summary>
        Task<SchedulingSummary> GetSchedulingSummaryAsync();

        /// <summary>
        /// Получает количество карточек для повторения по дням в указанном диапазоне дат.
        /// </summary>
        Task<Dictionary<DateTime, int>> GetCardsCountByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получает рекомендованное время для изучения на сегодня.
        /// </summary>
        Task<RecommendedStudyTime> GetRecommendedStudyTimeAsync();

        /// <summary>
        /// Получает оптимальное распределение карточек по дням недели.
        /// </summary>
        Task<WeeklyDistribution> GetOptimalWeeklyDistributionAsync();

        #endregion

        #region Управление уведомлениями

        /// <summary>
        /// Включает или выключает уведомления о повторениях.
        /// </summary>
        /// <param name="enabled">Включены ли уведомления.</param>
        Task SetNotificationsEnabledAsync(bool enabled);

        /// <summary>
        /// Устанавливает время для ежедневных уведомлений.
        /// </summary>
        /// <param name="hour">Час (0-23).</param>
        /// <param name="minute">Минута (0-59).</param>
        Task SetDailyNotificationTimeAsync(int hour, int minute);

        /// <summary>
        /// Отправляет уведомление о карточках для повторения сегодня.
        /// </summary>
        Task SendDailyReminderNotificationAsync();

        /// <summary>
        /// Проверяет, есть ли просроченные карточки, и отправляет уведомление при необходимости.
        /// </summary>
        Task CheckAndNotifyOverdueCardsAsync();

        /// <summary>
        /// Получает настройки уведомлений.
        /// </summary>
        Task<NotificationSettings> GetNotificationSettingsAsync();

        /// <summary>
        /// Сохраняет настройки уведомлений.
        /// </summary>
        Task SaveNotificationSettingsAsync(NotificationSettings settings);

        #endregion

        #region Планировщик повторений

        /// <summary>
        /// Создает план повторений на указанный период.
        /// </summary>
        /// <param name="startDate">Начальная дата.</param>
        /// <param name="endDate">Конечная дата.</param>
        /// <param name="maxCardsPerDay">Максимальное количество карточек в день.</param>
        Task<StudyPlan> CreateStudyPlanAsync(DateTime startDate, DateTime endDate, int maxCardsPerDay = 20);

        /// <summary>
        /// Оптимизирует расписание повторений для минимизации нагрузки.
        /// </summary>
        Task OptimizeScheduleAsync();

        /// <summary>
        /// Сбрасывает все запланированные повторения (используется после длительного перерыва).
        /// </summary>
        Task ResetAllSchedulesAsync();

        /// <summary>
        /// Адаптирует расписание под доступное время пользователя.
        /// </summary>
        /// <param name="availableTimePerDay">Доступное время в минутах в день.</param>
        Task AdaptScheduleToAvailableTimeAsync(int availableTimePerDay);

        #endregion

        #region Интеграция с календарем

        /// <summary>
        /// Экспортирует план повторений в календарь.
        /// </summary>
        Task<bool> ExportToCalendarAsync();

        /// <summary>
        /// Импортирует события из календаря для учета в расписании.
        /// </summary>
        Task<bool> ImportFromCalendarAsync();

        /// <summary>
        /// Синхронизирует расписание с системным календарем.
        /// </summary>
        Task SyncWithSystemCalendarAsync();

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Рассчитывает оптимальное время для следующего повторения на основе истории изучения.
        /// </summary>
        Task<DateTime> CalculateOptimalReviewTimeAsync(int cardId);

        /// <summary>
        /// Получает рекомендуемую продолжительность сессии изучения на сегодня.
        /// </summary>
        Task<TimeSpan> GetRecommendedSessionDurationAsync();

        /// <summary>
        /// Инициализирует сервис планирования.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Очищает кэш и временные данные.
        /// </summary>
        Task ClearCacheAsync();

        #endregion
    }

    /// <summary>
    /// Сводка по планированию повторений.
    /// </summary>
    public class SchedulingSummary
    {
        public int TotalCards { get; set; }
        public int CardsDueToday { get; set; }
        public int CardsDueTomorrow { get; set; }
        public int CardsDueThisWeek { get; set; }
        public int CardsDueNextWeek { get; set; }
        public int OverdueCards { get; set; }
        public DateTime NextReviewDate { get; set; }
        public DateTime LastReviewDate { get; set; }
        public double AverageCardsPerDay { get; set; }
        public int MaxCardsInSingleDay { get; set; }
        public DateTime MaxCardsDate { get; set; }
    }

    /// <summary>
    /// Рекомендованное время для изучения.
    /// </summary>
    public class RecommendedStudyTime
    {
        public TimeSpan BestTimeOfDay { get; set; }
        public int RecommendedDurationMinutes { get; set; }
        public int CardsToReview { get; set; }
        public DateTime SuggestedStartTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Оптимальное распределение по дням недели.
    /// </summary>
    public class WeeklyDistribution
    {
        public Dictionary<DayOfWeek, int> CardsPerDay { get; set; } = new();
        public Dictionary<DayOfWeek, TimeSpan> TimePerDay { get; set; } = new();
        public DayOfWeek BusiestDay { get; set; }
        public DayOfWeek LightestDay { get; set; }
        public double BalanceScore { get; set; }
    }

    /// <summary>
    /// Настройки уведомлений.
    /// </summary>
    public class NotificationSettings
    {
        public bool Enabled { get; set; } = true;
        public int DailyNotificationHour { get; set; } = 19; // 7 PM по умолчанию
        public int DailyNotificationMinute { get; set; } = 0;
        public bool NotifyOnOverdue { get; set; } = true;
        public bool NotifyBeforeReview { get; set; } = false;
        public int NotifyBeforeHours { get; set; } = 2;
        public bool UseSound { get; set; } = true;
        public bool UseVibration { get; set; } = true;
        public string NotificationTitle { get; set; } = "ChineseVocab - время повторить иероглифы";
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// План изучения на период.
    /// </summary>
    public class StudyPlan
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<DateTime, List<Card>> DailyCards { get; set; } = new();
        public int TotalCards { get; set; }
        public int TotalDays { get; set; }
        public double AverageCardsPerDay { get; set; }
        public bool IsFeasible { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
