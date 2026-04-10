using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Реализация сервиса планирования повторений карточек.
    /// Использует алгоритмы SRS для оптимизации расписания повторений и управления уведомлениями.
    /// </summary>
    public class SchedulerService : ISchedulerService
    {
        private readonly ISRSService _srsService;
        private readonly IDatabaseService _databaseService;
        private readonly INotificationService _notificationService;
        private NotificationSettings _notificationSettings;
        private Dictionary<DateTime, List<Card>> _cachedSchedule;
        private DateTime _cacheLastUpdated;
        private const int CacheValidityMinutes = 5;

        /// <summary>
        /// Конструктор сервиса планирования.
        /// </summary>
        public SchedulerService(ISRSService srsService, IDatabaseService databaseService)
        {
            _srsService = srsService ?? throw new ArgumentNullException(nameof(srsService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

            // Инициализация сервиса уведомлений (будет реализован отдельно)
            _notificationService = null; // TODO: Реализовать сервис уведомлений

            _notificationSettings = new NotificationSettings();
            _cachedSchedule = new Dictionary<DateTime, List<Card>>();
            _cacheLastUpdated = DateTime.MinValue;
        }

        #region Основные операции планирования

        /// <summary>
        /// Получает карточки, которые нужно повторить в указанный день.
        /// </summary>
        public async Task<List<Card>> GetCardsForDateAsync(DateTime date)
        {
            await EnsureInitializedAsync();

            // Проверяем кэш
            if (IsCacheValid() && _cachedSchedule.ContainsKey(date.Date))
            {
                return _cachedSchedule[date.Date];
            }

            // Получаем всю статистику SRS
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            // Фильтруем по дате следующего повторения
            var statsForDate = allStats
                .Where(s => s.NextReviewDate.Date == date.Date)
                .ToList();

            // Получаем карточки для найденной статистики
            var cardsForDate = new List<Card>();
            foreach (var stat in statsForDate)
            {
                var card = await _databaseService.GetCardByIdAsync(stat.CardId);
                if (card != null && card.IsActive)
                {
                    cardsForDate.Add(card);
                }
            }

            // Обновляем кэш
            UpdateCache(date.Date, cardsForDate);

            return cardsForDate;
        }

        /// <summary>
        /// Получает карточки, которые нужно повторить сегодня.
        /// </summary>
        public async Task<List<Card>> GetTodayCardsAsync()
        {
            return await GetCardsForDateAsync(DateTime.UtcNow.Date);
        }

        /// <summary>
        /// Получает карточки, которые нужно повторить завтра.
        /// </summary>
        public async Task<List<Card>> GetTomorrowCardsAsync()
        {
            return await GetCardsForDateAsync(DateTime.UtcNow.AddDays(1).Date);
        }

        /// <summary>
        /// Получает карточки, которые нужно повторить на следующей неделе.
        /// </summary>
        public async Task<List<Card>> GetNextWeekCardsAsync()
        {
            await EnsureInitializedAsync();

            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate = startDate.AddDays(7);

            return await GetCardsForDateRangeAsync(startDate, endDate);
        }

        /// <summary>
        /// Получает карточки, которые нужно повторить в течение следующих N дней.
        /// </summary>
        public async Task<List<Card>> GetCardsForNextDaysAsync(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Количество дней должно быть положительным числом.", nameof(days));

            await EnsureInitializedAsync();

            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(days);

            return await GetCardsForDateRangeAsync(startDate, endDate);
        }

        /// <summary>
        /// Планирует следующее повторение для карточки.
        /// </summary>
        public async Task ScheduleNextReviewAsync(int cardId, DateTime nextReviewDate)
        {
            await EnsureInitializedAsync();

            var stat = await _srsService.GetSRSStatForCardAsync(cardId);
            if (stat == null)
            {
                // Создаем новую статистику для карточки
                stat = new SRSStat(cardId);
            }

            stat.NextReviewDate = nextReviewDate;
            stat.ModifiedDate = DateTime.UtcNow;

            await _srsService.SaveSRSStatAsync(stat);

            // Инвалидируем кэш
            InvalidateCache();
        }

        /// <summary>
        /// Переносит повторение карточки на указанное количество дней.
        /// </summary>
        public async Task PostponeCardAsync(int cardId, int daysToPostpone)
        {
            await EnsureInitializedAsync();

            var stat = await _srsService.GetSRSStatForCardAsync(cardId);
            if (stat == null)
            {
                throw new ArgumentException($"Карточка с ID {cardId} не найдена.", nameof(cardId));
            }

            stat.NextReviewDate = stat.NextReviewDate.AddDays(daysToPostpone);
            stat.ModifiedDate = DateTime.UtcNow;

            await _srsService.SaveSRSStatAsync(stat);

            // Инвалидируем кэш
            InvalidateCache();
        }

        #endregion

        #region Статистика и аналитика планирования

        /// <summary>
        /// Получает сводку по планированию повторений.
        /// </summary>
        public async Task<SchedulingSummary> GetSchedulingSummaryAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var allCards = await _databaseService.GetAllCardsAsync();

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var nextWeekStart = today.AddDays(1);
            var nextWeekEnd = nextWeekStart.AddDays(7);

            var cardsDueToday = allStats.Count(s => s.NextReviewDate.Date == today);
            var cardsDueTomorrow = allStats.Count(s => s.NextReviewDate.Date == tomorrow);
            var cardsDueThisWeek = allStats.Count(s => s.NextReviewDate.Date >= today && s.NextReviewDate.Date <= today.AddDays(7));
            var cardsDueNextWeek = allStats.Count(s => s.NextReviewDate.Date >= nextWeekStart && s.NextReviewDate.Date <= nextWeekEnd);
            var overdueCards = allStats.Count(s => s.IsOverdue());

            // Рассчитываем среднее количество карточек в день
            var cardsByDate = allStats
                .GroupBy(s => s.NextReviewDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            double averageCardsPerDay = cardsByDate.Count > 0 ? cardsByDate.Average(g => g.Count) : 0;

            // Находим день с максимальным количеством карточек
            var maxCardsDay = cardsByDate.OrderByDescending(g => g.Count).FirstOrDefault();

            // Находим даты следующего и последнего повторения
            var nextReviewDate = allStats.Count > 0 ? allStats.Min(s => s.NextReviewDate) : DateTime.MaxValue;
            var lastReviewDate = allStats.Count > 0 ? allStats.Max(s => s.LastReviewed) : DateTime.MinValue;

            return new SchedulingSummary
            {
                TotalCards = allCards.Count,
                CardsDueToday = cardsDueToday,
                CardsDueTomorrow = cardsDueTomorrow,
                CardsDueThisWeek = cardsDueThisWeek,
                CardsDueNextWeek = cardsDueNextWeek,
                OverdueCards = overdueCards,
                NextReviewDate = nextReviewDate,
                LastReviewDate = lastReviewDate,
                AverageCardsPerDay = averageCardsPerDay,
                MaxCardsInSingleDay = maxCardsDay?.Count ?? 0,
                MaxCardsDate = maxCardsDay?.Date ?? DateTime.MinValue
            };
        }

        /// <summary>
        /// Получает количество карточек для повторения по дням в указанном диапазоне дат.
        /// </summary>
        public async Task<Dictionary<DateTime, int>> GetCardsCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            var result = new Dictionary<DateTime, int>();

            // Инициализируем все даты в диапазоне с нулевыми значениями
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                result[date] = 0;
            }

            // Заполняем фактическими значениями
            var statsInRange = allStats
                .Where(s => s.NextReviewDate.Date >= startDate.Date && s.NextReviewDate.Date <= endDate.Date)
                .GroupBy(s => s.NextReviewDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() });

            foreach (var group in statsInRange)
            {
                result[group.Date] = group.Count;
            }

            return result;
        }

        /// <summary>
        /// Получает рекомендованное время для изучения на сегодня.
        /// </summary>
        public async Task<RecommendedStudyTime> GetRecommendedStudyTimeAsync()
        {
            await EnsureInitializedAsync();

            var cardsDueToday = await GetTodayCardsAsync();
            var cardCount = cardsDueToday.Count;

            // Базовое время: 30 секунд на карточку
            int estimatedMinutes = (int)Math.Ceiling(cardCount * 0.5);

            // Минимум 10 минут, максимум 60 минут
            int recommendedDuration = Math.Clamp(estimatedMinutes, 10, 60);

            // Рекомендуемое время дня: 19:00 (вечером)
            var bestTimeOfDay = new TimeSpan(19, 0, 0);

            // Предлагаем начать через 2 часа от текущего времени, но не раньше 18:00
            var now = DateTime.Now;
            var suggestedStart = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0);

            // Если уже позже 21:00, предлагаем на завтра
            if (now.Hour >= 21)
            {
                suggestedStart = suggestedStart.AddDays(1);
            }
            // Если сейчас раньше 18:00, предлагаем на 19:00 сегодня
            else if (now.Hour < 18)
            {
                // Оставляем suggestedStart как есть (19:00 сегодня)
            }
            else
            {
                // Сейчас между 18:00 и 21:00, предлагаем начать через 30 минут
                suggestedStart = now.AddMinutes(30);
            }

            string reason = cardCount == 0
                ? "Сегодня нет карточек для повторения. Отличный день для изучения новых карточек!"
                : $"Сегодня {cardCount} карточек для повторения. Рекомендуется уделить {recommendedDuration} минут.";

            return new RecommendedStudyTime
            {
                BestTimeOfDay = bestTimeOfDay,
                RecommendedDurationMinutes = recommendedDuration,
                CardsToReview = cardCount,
                SuggestedStartTime = suggestedStart,
                Reason = reason
            };
        }

        /// <summary>
        /// Получает оптимальное распределение карточек по дням недели.
        /// </summary>
        public async Task<WeeklyDistribution> GetOptimalWeeklyDistributionAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            var distribution = new WeeklyDistribution();

            // Группируем карточки по дню недели
            var cardsByDayOfWeek = allStats
                .GroupBy(s => s.NextReviewDate.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Day, g => g.Count);

            // Инициализируем все дни недели
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                distribution.CardsPerDay[day] = cardsByDayOfWeek.ContainsKey(day) ? cardsByDayOfWeek[day] : 0;

                // Предполагаем 30 секунд на карточку
                distribution.TimePerDay[day] = TimeSpan.FromSeconds(distribution.CardsPerDay[day] * 30);
            }

            // Находим самый загруженный и самый легкий день
            distribution.BusiestDay = distribution.CardsPerDay.OrderByDescending(kv => kv.Value).First().Key;
            distribution.LightestDay = distribution.CardsPerDay.OrderBy(kv => kv.Value).First().Key;

            // Рассчитываем коэффициент баланса (0-1, где 1 - идеальный баланс)
            double average = distribution.CardsPerDay.Values.Average();
            double variance = distribution.CardsPerDay.Values.Sum(count => Math.Pow(count - average, 2));
            double stdDev = Math.Sqrt(variance / distribution.CardsPerDay.Count);

            // Нормализуем в диапазон 0-1 (чем меньше отклонение, тем лучше)
            distribution.BalanceScore = Math.Max(0, 1 - (stdDev / (average > 0 ? average : 1)));

            return distribution;
        }

        #endregion

        #region Управление уведомлениями

        /// <summary>
        /// Включает или выключает уведомления о повторениях.
        /// </summary>
        public Task SetNotificationsEnabledAsync(bool enabled)
        {
            _notificationSettings.Enabled = enabled;
            _notificationSettings.LastModified = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Устанавливает время для ежедневных уведомлений.
        /// </summary>
        public Task SetDailyNotificationTimeAsync(int hour, int minute)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour), "Час должен быть в диапазоне 0-23.");
            if (minute < 0 || minute > 59)
                throw new ArgumentOutOfRangeException(nameof(minute), "Минута должна быть в диапазоне 0-59.");

            _notificationSettings.DailyNotificationHour = hour;
            _notificationSettings.DailyNotificationMinute = minute;
            _notificationSettings.LastModified = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Отправляет уведомление о карточках для повторения сегодня.
        /// </summary>
        public async Task SendDailyReminderNotificationAsync()
        {
            if (_notificationService == null || !_notificationSettings.Enabled)
                return;

            var cardsDueToday = await GetTodayCardsAsync();
            var overdueCards = await GetOverdueCardsAsync();

            string title = "ChineseVocab - время повторить иероглифы";
            string message;

            if (cardsDueToday.Count == 0 && overdueCards.Count == 0)
            {
                message = "Сегодня нет карточек для повторения. Отличный день для изучения новых!";
            }
            else if (overdueCards.Count > 0)
            {
                message = $"Внимание! У вас {overdueCards.Count} просроченных карточек и {cardsDueToday.Count} на сегодня.";
            }
            else
            {
                message = $"Сегодня {cardsDueToday.Count} карточек для повторения. Уделите несколько минут!";
            }

            // TODO: Реализовать отправку уведомления через _notificationService
            // await _notificationService.SendNotificationAsync(title, message);
        }

        /// <summary>
        /// Проверяет, есть ли просроченные карточки, и отправляет уведомление при необходимости.
        /// </summary>
        public async Task CheckAndNotifyOverdueCardsAsync()
        {
            if (_notificationService == null || !_notificationSettings.NotifyOnOverdue)
                return;

            var overdueCards = await GetOverdueCardsAsync();

            if (overdueCards.Count > 0)
            {
                string title = "ChineseVocab - просроченные карточки";
                string message = $"У вас {overdueCards.Count} просроченных карточек. Не забудьте их повторить!";

                // TODO: Реализовать отправку уведомления через _notificationService
                // await _notificationService.SendNotificationAsync(title, message);
            }
        }

        /// <summary>
        /// Получает настройки уведомлений.
        /// </summary>
        public Task<NotificationSettings> GetNotificationSettingsAsync()
        {
            return Task.FromResult(_notificationSettings);
        }

        /// <summary>
        /// Сохраняет настройки уведомлений.
        /// </summary>
        public Task SaveNotificationSettingsAsync(NotificationSettings settings)
        {
            _notificationSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _notificationSettings.LastModified = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        #endregion

        #region Планировщик повторений

        /// <summary>
        /// Создает план повторений на указанный период.
        /// </summary>
        public async Task<StudyPlan> CreateStudyPlanAsync(DateTime startDate, DateTime endDate, int maxCardsPerDay = 20)
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var plan = new StudyPlan
            {
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                DailyCards = new Dictionary<DateTime, List<Card>>(),
                TotalDays = (int)(endDate.Date - startDate.Date).TotalDays + 1
            };

            // Инициализируем все дни с пустыми списками
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                plan.DailyCards[date] = new List<Card>();
            }

            // Распределяем карточки по дням
            var statsByDate = allStats
                .Where(s => s.NextReviewDate.Date >= startDate.Date && s.NextReviewDate.Date <= endDate.Date)
                .GroupBy(s => s.NextReviewDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Перераспределяем карточки, если в какой-то день их слишком много
            foreach (var date in plan.DailyCards.Keys.OrderBy(d => d))
            {
                if (statsByDate.ContainsKey(date))
                {
                    var cardsForDate = statsByDate[date];

                    if (cardsForDate.Count <= maxCardsPerDay)
                    {
                        // Все карточки помещаются в этот день
                        foreach (var stat in cardsForDate)
                        {
                            var card = await _databaseService.GetCardByIdAsync(stat.CardId);
                            if (card != null)
                            {
                                plan.DailyCards[date].Add(card);
                            }
                        }
                    }
                    else
                    {
                        // Слишком много карточек, распределяем по следующим дням
                        int cardsToMove = cardsForDate.Count - maxCardsPerDay;
                        var cardsToRedistribute = cardsForDate
                            .OrderBy(s => s.Interval) // Сначала карточки с маленькими интервалами
                            .Take(cardsToMove)
                            .ToList();

                        // Добавляем карточки, которые остаются в этот день
                        foreach (var stat in cardsForDate.Except(cardsToRedistribute))
                        {
                            var card = await _databaseService.GetCardByIdAsync(stat.CardId);
                            if (card != null)
                            {
                                plan.DailyCards[date].Add(card);
                            }
                        }

                        // Перераспределяем оставшиеся карточки
                        int currentIndex = 0;
                        var nextDate = date.AddDays(1);

                        while (currentIndex < cardsToRedistribute.Count && nextDate <= endDate)
                        {
                            int availableSlots = maxCardsPerDay - plan.DailyCards[nextDate].Count;
                            int cardsToAdd = Math.Min(availableSlots, cardsToRedistribute.Count - currentIndex);

                            for (int i = 0; i < cardsToAdd; i++)
                            {
                                var stat = cardsToRedistribute[currentIndex];
                                var card = await _databaseService.GetCardByIdAsync(stat.CardId);
                                if (card != null)
                                {
                                    plan.DailyCards[nextDate].Add(card);
                                }
                                currentIndex++;
                            }

                            nextDate = nextDate.AddDays(1);
                        }

                        // Если все еще есть карточки для распределения, добавляем их в примечания
                        if (currentIndex < cardsToRedistribute.Count)
                        {
                            plan.Notes += $"Внимание: {cardsToRedistribute.Count - currentIndex} карточек не удалось распределить в пределах плана. ";
                        }
                    }
                }
            }

            // Рассчитываем итоговую статистику
            plan.TotalCards = plan.DailyCards.Values.Sum(cards => cards.Count);
            plan.AverageCardsPerDay = plan.TotalDays > 0 ? (double)plan.TotalCards / plan.TotalDays : 0;
            plan.IsFeasible = plan.DailyCards.Values.All(cards => cards.Count <= maxCardsPerDay * 1.5); // Допускаем 50% перегрузки

            if (!plan.IsFeasible)
            {
                plan.Notes += "План может быть слишком напряженным. Рекомендуется увеличить период или уменьшить количество карточек в день.";
            }

            return plan;
        }

        /// <summary>
        /// Оптимизирует расписание повторений для минимизации нагрузки.
        /// </summary>
        public async Task OptimizeScheduleAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            // Группируем карточки по дате
            var statsByDate = allStats
                .GroupBy(s => s.NextReviewDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Находим дни с наибольшей нагрузкой
            var busyDays = statsByDate
                .Where(kv => kv.Value.Count > 15) // Порог: больше 15 карточек в день
                .OrderByDescending(kv => kv.Value.Count)
                .ToList();

            foreach (var busyDay in busyDays)
            {
                var date = busyDay.Key;
                var cards = busyDay.Value;

                // Сортируем карточки по приоритету (сначала те, которые можно перенести)
                var movableCards = cards
                    .Where(s => s.Interval > 1) // Не переносим карточки с интервалом 1 день
                    .OrderBy(s => s.Interval)   // Сначала карточки с маленькими интервалами
                    .ThenBy(s => s.EFactor)     // Затем с низким E-Factor
                    .ToList();

                // Пытаемся перенести часть карточек на соседние дни
                int cardsToMove = Math.Max(1, cards.Count - 15); // Стараемся оставить не более 15 карточек в день
                int moved = 0;

                // Пробуем перенести на предыдущий день
                var prevDate = date.AddDays(-1);
                if (statsByDate.ContainsKey(prevDate) && statsByDate[prevDate].Count < 10)
                {
                    int slotsAvailable = 10 - statsByDate[prevDate].Count;
                    int canMove = Math.Min(cardsToMove, Math.Min(slotsAvailable, movableCards.Count));

                    for (int i = 0; i < canMove && moved < cardsToMove; i++)
                    {
                        var stat = movableCards[i];
                        stat.NextReviewDate = prevDate;
                        moved++;
                    }
                }

                // Пробуем перенести на следующий день
                var nextDate = date.AddDays(1);
                if (moved < cardsToMove && statsByDate.ContainsKey(nextDate) && statsByDate[nextDate].Count < 10)
                {
                    int slotsAvailable = 10 - statsByDate[nextDate].Count;
                    int canMove = Math.Min(cardsToMove - moved, Math.Min(slotsAvailable, movableCards.Count - moved));

                    for (int i = moved; i < moved + canMove; i++)
                    {
                        var stat = movableCards[i];
                        stat.NextReviewDate = nextDate;
                        moved++;
                    }
                }

                // Сохраняем изменения
                foreach (var stat in movableCards.Take(moved))
                {
                    await _srsService.SaveSRSStatAsync(stat);
                }
            }

            // Инвалидируем кэш
            InvalidateCache();
        }

        /// <summary>
        /// Сбрасывает все запланированные повторения (используется после длительного перерыва).
        /// </summary>
        public async Task ResetAllSchedulesAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var today = DateTime.UtcNow.Date;

            foreach (var stat in allStats)
            {
                // Переносим все повторения на сегодня или ближайшие дни
                if (stat.NextReviewDate < today)
                {
                    // Просроченные карточки - на сегодня
                    stat.NextReviewDate = today;
                }
                else if ((stat.NextReviewDate - today).TotalDays > 7)
                {
                    // Карточки запланированные больше чем на неделю вперед - переносим на ближайшие 3 дня
                    stat.NextReviewDate = today.AddDays(new Random().Next(1, 4));
                }

                stat.ModifiedDate = DateTime.UtcNow;
                await _srsService.SaveSRSStatAsync(stat);
            }

            // Инвалидируем кэш
            InvalidateCache();
        }

        /// <summary>
        /// Адаптирует расписание под доступное время пользователя.
        /// </summary>
        public async Task AdaptScheduleToAvailableTimeAsync(int availableTimePerDay)
        {
            if (availableTimePerDay <= 0)
                throw new ArgumentException("Доступное время должно быть положительным числом.", nameof(availableTimePerDay));

            await EnsureInitializedAsync();

            // Предполагаем, что на карточку нужно 30 секунд
            int maxCardsPerDay = availableTimePerDay * 2; // 30 секунд на карточку = 2 карточки в минуту

            // Получаем текущее расписание на ближайшие 30 дней
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(30);

            var cardsByDate = await GetCardsCountByDateRangeAsync(startDate, endDate);

            // Находим дни с перегрузкой
            foreach (var kvp in cardsByDate.Where(kv => kv.Value > maxCardsPerDay))
            {
                var date = kvp.Key;
                int excessCards = kvp.Value - maxCardsPerDay;

                // Получаем карточки на эту дату
                var cardsForDate = await GetCardsForDateAsync(date);

                // Сортируем по приоритету переноса (сначала карточки с большими интервалами)
                var sortedCards = cardsForDate
                    .Select(c => new { Card = c, Stat = _srsService.GetSRSStatForCardAsync(c.Id).Result })
                    .Where(x => x.Stat != null)
                    .OrderByDescending(x => x.Stat.Interval)
                    .ThenBy(x => x.Stat.EFactor)
                    .Take(excessCards)
                    .ToList();

                // Переносим на ближайшие доступные дни
                foreach (var cardData in sortedCards)
                {
                    // Ищем ближайший день с доступными слотами
                    DateTime newDate = date;
                    for (int offset = 1; offset <= 7; offset++)
                    {
                        newDate = date.AddDays(offset);
                        if (newDate > endDate)
                            break;

                        if (cardsByDate.ContainsKey(newDate) && cardsByDate[newDate] < maxCardsPerDay)
                        {
                            // Нашли день с доступными слотами
                            await PostponeCardAsync(cardData.Card.Id, offset);
                            cardsByDate[date]--;
                            cardsByDate[newDate]++;
                            break;
                        }
                    }
                }
            }

            // Инвалидируем кэш
            InvalidateCache();
        }

        #endregion

        #region Интеграция с календарем

        /// <summary>
        /// Экспортирует план повторений в календарь.
        /// </summary>
        public Task<bool> ExportToCalendarAsync()
        {
            // TODO: Реализовать экспорт в календарь
            // Для .NET MAUI можно использовать платформенные API для календаря
            return Task.FromResult(false);
        }

        /// <summary>
        /// Импортирует события из календаря для учета в расписании.
        /// </summary>
        public Task<bool> ImportFromCalendarAsync()
        {
            // TODO: Реализовать импорт из календаря
            return Task.FromResult(false);
        }

        /// <summary>
        /// Синхронизирует расписание с системным календарем.
        /// </summary>
        public Task SyncWithSystemCalendarAsync()
        {
            // TODO: Реализовать синхронизацию с календарем
            return Task.CompletedTask;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Рассчитывает оптимальное время для следующего повторения на основе истории изучения.
        /// </summary>
        public async Task<DateTime> CalculateOptimalReviewTimeAsync(int cardId)
        {
            await EnsureInitializedAsync();

            var stat = await _srsService.GetSRSStatForCardAsync(cardId);
            if (stat == null)
            {
                // Если статистики нет, рекомендуем повторить сегодня вечером
                return DateTime.UtcNow.Date.AddHours(19);
            }

            // Анализируем историю повторений
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var cardStats = allStats.Where(s => s.CardId == cardId).ToList();

            if (cardStats.Count == 0)
            {
                // Первое повторение - сегодня вечером
                return DateTime.UtcNow.Date.AddHours(19);
            }

            // Анализируем время дня, когда пользователь обычно занимается
            // TODO: Собирать и анализировать историю времени занятий

            // Пока возвращаем стандартное время: 19:00
            return stat.NextReviewDate.Date.AddHours(19);
        }

        /// <summary>
        /// Получает рекомендуемую продолжительность сессии изучения на сегодня.
        /// </summary>
        public async Task<TimeSpan> GetRecommendedSessionDurationAsync()
        {
            var recommendedTime = await GetRecommendedStudyTimeAsync();
            return TimeSpan.FromMinutes(recommendedTime.RecommendedDurationMinutes);
        }

        /// <summary>
        /// Инициализирует сервис планирования.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Загружаем настройки уведомлений (в будущем из базы данных или файла)
            _notificationSettings = new NotificationSettings();

            // Инициализируем зависимости
            await _srsService.InitializeAsync();

            // Очищаем кэш
            InvalidateCache();
        }

        /// <summary>
        /// Очищает кэш и временные данные.
        /// </summary>
        public Task ClearCacheAsync()
        {
            InvalidateCache();
            return Task.CompletedTask;
        }

        #endregion

        #region Приватные вспомогательные методы

        /// <summary>
        /// Получает карточки для диапазона дат.
        /// </summary>
        private async Task<List<Card>> GetCardsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            var statsInRange = allStats
                .Where(s => s.NextReviewDate.Date >= startDate.Date && s.NextReviewDate.Date <= endDate.Date)
                .ToList();

            var cards = new List<Card>();
            foreach (var stat in statsInRange)
            {
                var card = await _databaseService.GetCardByIdAsync(stat.CardId);
                if (card != null && card.IsActive)
                {
                    cards.Add(card);
                }
            }

            return cards;
        }

        /// <summary>
        /// Получает просроченные карточки.
        /// </summary>
        private async Task<List<Card>> GetOverdueCardsAsync()
        {
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            var overdueStats = allStats
                .Where(s => s.IsOverdue())
                .ToList();

            var cards = new List<Card>();
            foreach (var stat in overdueStats)
            {
                var card = await _databaseService.GetCardByIdAsync(stat.CardId);
                if (card != null && card.IsActive)
                {
                    cards.Add(card);
                }
            }

            return cards;
        }

        /// <summary>
        /// Проверяет, инициализирован ли сервис.
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            // В текущей реализации всегда инициализирован после конструктора
            // В будущем можно добавить более сложную логику инициализации
            await Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет кэш расписания.
        /// </summary>
        private void UpdateCache(DateTime date, List<Card> cards)
        {
            _cachedSchedule[date] = cards;
            _cacheLastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Проверяет валидность кэша.
        /// </summary>
        private bool IsCacheValid()
        {
            return (DateTime.UtcNow - _cacheLastUpdated).TotalMinutes < CacheValidityMinutes;
        }

        /// <summary>
        /// Инвалидирует кэш.
        /// </summary>
        private void InvalidateCache()
        {
            _cachedSchedule.Clear();
            _cacheLastUpdated = DateTime.MinValue;
        }

        #endregion
    }

    /// <summary>
    /// Интерфейс сервиса уведомлений (для будущей реализации).
    /// </summary>
    internal interface INotificationService
    {
        Task SendNotificationAsync(string title, string message);
        Task<bool> RequestNotificationPermissionAsync();
    }
}
