using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Реализация сервиса системы интервальных повторений (Spaced Repetition System).
    /// Использует алгоритм SM-2 для оптимизации запоминания китайских иероглифов.
    /// </summary>
    public class SRSService : ISRSService
    {
        private readonly IDatabaseService _databaseService;
        private SRSSettings _settings;
        private bool _isInitialized = false;

        /// <summary>
        /// Конструктор сервиса SRS.
        /// </summary>
        /// <param name="databaseService">Сервис базы данных для доступа к данным карточек и статистики.</param>
        public SRSService(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _settings = new SRSSettings(); // Настройки по умолчанию
        }

        #region Основные операции SRS

        /// <summary>
        /// Обрабатывает оценку карточки пользователем по алгоритму SM-2.
        /// </summary>
        public async Task<SRSStat> ProcessCardReviewAsync(int cardId, int quality)
        {
            await EnsureInitializedAsync();

            // Проверяем валидность оценки
            if (!IsValidQualityScore(quality))
            {
                throw new ArgumentOutOfRangeException(nameof(quality), "Оценка качества должна быть в диапазоне 0-5.");
            }

            // Получаем или создаем статистику для карточки
            var stat = await GetSRSStatForCardAsync(cardId) ?? new SRSStat(cardId);

            // Применяем алгоритм SM-2
            stat.ProcessSM2Review(quality);

            // Сохраняем обновленную статистику
            await SaveSRSStatAsync(stat);

            return stat;
        }

        /// <summary>
        /// Получает карточки, которые нужно повторить (дата следующего повторения наступила).
        /// </summary>
        public async Task<List<Card>> GetDueCardsAsync(int limit = 0)
        {
            await EnsureInitializedAsync();

            // Получаем всю статистику SRS
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            // Фильтруем по дате следующего повторения
            var dueStats = allStats.Where(s => s.IsDueForReview()).ToList();

            // Получаем карточки для найденной статистики
            var dueCards = new List<Card>();
            foreach (var stat in dueStats)
            {
                var card = await _databaseService.GetCardByIdAsync(stat.CardId);
                if (card != null && card.IsActive)
                {
                    dueCards.Add(card);
                }

                // Применяем лимит, если указан
                if (limit > 0 && dueCards.Count >= limit)
                {
                    break;
                }
            }

            return dueCards;
        }

        /// <summary>
        /// Получает карточки из определенной колоды, которые нужно повторить.
        /// </summary>
        public async Task<List<Card>> GetDueCardsByDeckAsync(int deckId, int limit = 0)
        {
            await EnsureInitializedAsync();

            // Временная заглушка - возвращаем карточки для повторения без фильтрации по колоде
            var dueCards = await GetDueCardsAsync(limit);
            return dueCards;
        }

        /// <summary>
        /// Получает карточки для изучения сегодня (новые карточки + карточки для повторения).
        /// </summary>
        public async Task<List<Card>> GetStudySessionCardsAsync(int newCardsLimit = 10, int reviewCardsLimit = 20)
        {
            await EnsureInitializedAsync();

            var sessionCards = new List<Card>();

            // 1. Получаем карточки для повторения
            var reviewCards = await GetDueCardsAsync(reviewCardsLimit);
            sessionCards.AddRange(reviewCards);

            // 2. Получаем новые карточки (те, для которых еще нет статистики SRS)
            var allCards = await _databaseService.GetAllCardsAsync();
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            var cardsWithStats = allStats.Select(s => s.CardId).ToHashSet();
            var newCards = allCards
                .Where(c => !cardsWithStats.Contains(c.Id))
                .Take(newCardsLimit)
                .ToList();

            sessionCards.AddRange(newCards);

            // Перемешиваем карточки для разнообразия
            var random = new Random();
            return sessionCards.OrderBy(c => random.Next()).ToList();
        }

        /// <summary>
        /// Получает статистику SRS для карточки.
        /// </summary>
        public async Task<SRSStat> GetSRSStatForCardAsync(int cardId)
        {
            await EnsureInitializedAsync();
            return await _databaseService.GetSRSStatisticsByCardIdAsync(cardId);
        }

        /// <summary>
        /// Создает или обновляет статистику SRS для карточки.
        /// </summary>
        public async Task SaveSRSStatAsync(SRSStat stat)
        {
            await EnsureInitializedAsync();
            await _databaseService.SaveSRSStatisticsAsync(stat);
        }

        /// <summary>
        /// Сбрасывает прогресс изучения карточки.
        /// </summary>
        public async Task ResetCardProgressAsync(int cardId)
        {
            await EnsureInitializedAsync();

            var stat = await GetSRSStatForCardAsync(cardId);
            if (stat != null)
            {
                stat.Reset();
                await SaveSRSStatAsync(stat);
            }
        }

        /// <summary>
        /// Помечает карточку как выученную.
        /// </summary>
        public async Task MarkCardAsLearnedAsync(int cardId)
        {
            await EnsureInitializedAsync();

            var stat = await GetSRSStatForCardAsync(cardId);
            if (stat != null)
            {
                stat.IsLearned = true;
                stat.LearnedDate = DateTime.UtcNow;
                stat.Interval = Math.Max(stat.Interval, _settings.LearnedThreshold);
                await SaveSRSStatAsync(stat);
            }
        }

        #endregion

        #region Статистика и аналитика

        /// <summary>
        /// Получает сводку по изучению (количество карточек для повторения, изученных и т.д.).
        /// </summary>
        public async Task<SRSStudySummary> GetStudySummaryAsync()
        {
            await EnsureInitializedAsync();

            var allCards = await _databaseService.GetAllCardsAsync();
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            var dueCards = await GetDueCardsAsync();
            var dueTomorrow = allStats.Count(s => s.NextReviewDate.Date == DateTime.UtcNow.AddDays(1).Date);

            var learnedCards = allStats.Count(s => s.IsLearned);
            var newCards = allCards.Count(c => !allStats.Any(s => s.CardId == c.Id));

            var averageEFactor = allStats.Count > 0 ? allStats.Average(s => s.EFactor) : 0;
            var averageEaseScore = allStats.Count > 0 ? allStats.Average(s => s.EaseScore) : 0;

            // Рассчитываем общее время изучения (предполагаем 10 секунд на повторение)
            var totalStudySeconds = allStats.Sum(s => s.TotalStudyTimeSeconds);

            return new SRSStudySummary
            {
                TotalCards = allCards.Count,
                DueCardsToday = dueCards.Count,
                DueCardsTomorrow = dueTomorrow,
                NewCardsAvailable = newCards,
                LearnedCards = learnedCards,
                AverageEFactor = averageEFactor,
                AverageEaseScore = averageEaseScore,
                CurrentStreak = await GetCurrentStreakAsync(),
                MaxStreak = await GetMaxStreakAsync(),
                AccuracyPercentage = allStats.Count > 0 ?
                    allStats.Average(s => s.AccuracyPercentage) : 0,
                TotalStudyHours = totalStudySeconds / 3600.0,
                AverageTimePerCard = allStats.Count > 0 ?
                    allStats.Average(s => s.AverageResponseTime) : 0,
                LastStudyDate = allStats.Count > 0 ?
                    allStats.Max(s => s.LastReviewed) : DateTime.MinValue,
                StudyStreakDays = 0 // TODO: Реализовать расчет серии дней изучения
            };
        }

        /// <summary>
        /// Получает количество карточек для повторения на сегодня.
        /// </summary>
        public async Task<int> GetDueCardsCountAsync()
        {
            await EnsureInitializedAsync();
            var dueCards = await GetDueCardsAsync();
            return dueCards.Count;
        }

        /// <summary>
        /// Получает количество карточек для повторения на завтра.
        /// </summary>
        public async Task<int> GetDueCardsCountForTomorrowAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;

            return allStats.Count(s => s.NextReviewDate.Date == tomorrow);
        }

        /// <summary>
        /// Получает количество карточек для повторения на определенную дату.
        /// </summary>
        public async Task<int> GetDueCardsCountForDateAsync(DateTime date)
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            return allStats.Count(s => s.NextReviewDate.Date == date.Date);
        }

        /// <summary>
        /// Получает количество новых карточек, доступных для изучения.
        /// </summary>
        public async Task<int> GetNewCardsCountAsync()
        {
            await EnsureInitializedAsync();

            var allCards = await _databaseService.GetAllCardsAsync();
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var cardsWithStats = allStats.Select(s => s.CardId).ToHashSet();

            return allCards.Count(c => !cardsWithStats.Contains(c.Id));
        }

        /// <summary>
        /// Получает количество изученных карточек.
        /// </summary>
        public async Task<int> GetLearnedCardsCountAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            return allStats.Count(s => s.IsLearned);
        }

        /// <summary>
        /// Получает текущую серию правильных ответов подряд.
        /// </summary>
        public async Task<int> GetCurrentStreakAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            return allStats.Count > 0 ? allStats.Max(s => s.CorrectStreak) : 0;
        }

        /// <summary>
        /// Получает максимальную серию правильных ответов подряд.
        /// </summary>
        public async Task<int> GetMaxStreakAsync()
        {
            // Временная заглушка - в будущем будем хранить максимальную серию отдельно
            return await GetCurrentStreakAsync();
        }

        /// <summary>
        /// Получает средний фактор легкости (E-Factor) для всех карточек.
        /// </summary>
        public async Task<double> GetAverageEFactorAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            return allStats.Count > 0 ? allStats.Average(s => s.EFactor) : 0;
        }

        /// <summary>
        /// Получает среднюю оценку (Ease Score) для всех карточек.
        /// </summary>
        public async Task<double> GetAverageEaseScoreAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            return allStats.Count > 0 ? allStats.Average(s => s.EaseScore) : 0;
        }

        /// <summary>
        /// Получает статистику изучения по дням за указанный период.
        /// </summary>
        public async Task<List<SRSDailyStudyStats>> GetDailyStudyStatsAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();

            // Временная заглушка - возвращаем пустой список
            return new List<SRSDailyStudyStats>();
        }

        /// <summary>
        /// Получает статистику по уровням HSK.
        /// </summary>
        public async Task<List<HskLevelSRSStats>> GetHskLevelStatsAsync()
        {
            await EnsureInitializedAsync();

            // Временная заглушка
            return new List<HskLevelSRSStats>();
        }

        #endregion

        #region Настройки SRS

        /// <summary>
        /// Получает настройки SRS.
        /// </summary>
        public Task<SRSSettings> GetSettingsAsync()
        {
            return Task.FromResult(_settings);
        }

        /// <summary>
        /// Сохраняет настройки SRS.
        /// </summary>
        public Task SaveSettingsAsync(SRSSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.LastModified = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Сбрасывает настройки SRS к значениям по умолчанию.
        /// </summary>
        public Task ResetSettingsToDefaultAsync()
        {
            _settings = new SRSSettings();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает лимит новых карточек в день.
        /// </summary>
        public Task<int> GetDailyNewCardsLimitAsync()
        {
            return Task.FromResult(_settings.DailyNewCardsLimit);
        }

        /// <summary>
        /// Устанавливает лимит новых карточек в день.
        /// </summary>
        public Task SetDailyNewCardsLimitAsync(int limit)
        {
            _settings.DailyNewCardsLimit = Math.Max(0, limit);
            _settings.LastModified = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает лимит карточек для повторения в день.
        /// </summary>
        public Task<int> GetDailyReviewCardsLimitAsync()
        {
            return Task.FromResult(_settings.DailyReviewCardsLimit);
        }

        /// <summary>
        /// Устанавливает лимит карточек для повторения в день.
        /// </summary>
        public Task SetDailyReviewCardsLimitAsync(int limit)
        {
            _settings.DailyReviewCardsLimit = Math.Max(0, limit);
            _settings.LastModified = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает минимальный интервал для карточек (в днях).
        /// </summary>
        public Task<int> GetMinimumIntervalAsync()
        {
            return Task.FromResult(_settings.MinimumInterval);
        }

        /// <summary>
        /// Устанавливает минимальный интервал для карточек (в днях).
        /// </summary>
        public Task SetMinimumIntervalAsync(int days)
        {
            _settings.MinimumInterval = Math.Max(1, days);
            _settings.LastModified = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Получает максимальный интервал для карточек (в днях).
        /// </summary>
        public Task<int> GetMaximumIntervalAsync()
        {
            return Task.FromResult(_settings.MaximumInterval);
        }

        /// <summary>
        /// Устанавливает максимальный интервал для карточек (в днях).
        /// </summary>
        public Task SetMaximumIntervalAsync(int days)
        {
            _settings.MaximumInterval = Math.Max(_settings.MinimumInterval, days);
            _settings.LastModified = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        #endregion

        #region Утилиты и вспомогательные методы

        /// <summary>
        /// Рассчитывает следующую дату повторения на основе текущей статистики и оценки.
        /// </summary>
        public Task<DateTime> CalculateNextReviewDateAsync(SRSStat stat, int quality)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            var tempStat = new SRSStat(stat.CardId)
            {
                Interval = stat.Interval,
                EFactor = stat.EFactor,
                RepetitionCount = stat.RepetitionCount
            };

            tempStat.ProcessSM2Review(quality);
            return Task.FromResult(tempStat.NextReviewDate);
        }

        /// <summary>
        /// Рассчитывает новый интервал повторения по алгоритму SM-2.
        /// </summary>
        public Task<int> CalculateNextIntervalAsync(SRSStat stat, int quality)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            var tempStat = new SRSStat(stat.CardId)
            {
                Interval = stat.Interval,
                EFactor = stat.EFactor,
                RepetitionCount = stat.RepetitionCount
            };

            tempStat.ProcessSM2Review(quality);
            return Task.FromResult(tempStat.Interval);
        }

        /// <summary>
        /// Рассчитывает новый фактор легкости (E-Factor) по алгоритму SM-2.
        /// </summary>
        public Task<double> CalculateNewEFactorAsync(SRSStat stat, int quality)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));

            var tempStat = new SRSStat(stat.CardId)
            {
                Interval = stat.Interval,
                EFactor = stat.EFactor,
                RepetitionCount = stat.RepetitionCount
            };

            tempStat.ProcessSM2Review(quality);
            return Task.FromResult(tempStat.EFactor);
        }

        /// <summary>
        /// Проверяет, действительна ли оценка качества (0-5).
        /// </summary>
        public bool IsValidQualityScore(int quality)
        {
            return quality >= 0 && quality <= 5;
        }

        /// <summary>
        /// Конвертирует оценку качества в текстовое описание.
        /// </summary>
        public string QualityScoreToDescription(int quality)
        {
            return quality switch
            {
                0 => "Полный провал (совсем не помню)",
                1 => "Неправильный ответ (помню, но ошибся)",
                2 => "Почти правильно (с трудом вспомнил)",
                3 => "Правильно с затруднениями (вспомнил после размышлений)",
                4 => "Правильно легко (быстро вспомнил)",
                5 => "Правильно мгновенно (знаю на отлично)",
                _ => "Неизвестная оценка"
            };
        }

        /// <summary>
        /// Инициализирует сервис (загружает настройки, подготавливает данные).
        /// </summary>
        public async Task InitializeAsync()
        {
            if (!_isInitialized)
            {
                // Убеждаемся, что база данных инициализирована
                if (!await _databaseService.DatabaseExistsAsync())
                {
                    await _databaseService.InitializeDatabaseAsync();
                }

                // Загружаем настройки (в будущем из базы данных или файла)
                // Пока используем настройки по умолчанию

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Очищает кэш и временные данные.
        /// </summary>
        public Task ClearCacheAsync()
        {
            // В текущей реализации кэша нет
            return Task.CompletedTask;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет, инициализирован ли сервис, и если нет - инициализирует.
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }
        }

        #endregion
    }
}
