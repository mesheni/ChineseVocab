using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChineseVocab.Models;
using ChineseVocab.Services;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Реализация сервиса статистики и аналитики.
    /// Предоставляет методы для сбора, анализа и отображения статистики изучения китайского языка.
    /// Временная заглушка для демонстрации функциональности.
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ISRSService _srsService;
        private readonly ICharacterService _characterService;
        private readonly ISentenceService _sentenceService;
        private bool _isInitialized = false;

        /// <summary>
        /// Конструктор сервиса статистики.
        /// </summary>
        public StatisticsService(
            IDatabaseService databaseService,
            ISRSService srsService,
            ICharacterService characterService,
            ISentenceService sentenceService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _srsService = srsService ?? throw new ArgumentNullException(nameof(srsService));
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
            _sentenceService = sentenceService ?? throw new ArgumentNullException(nameof(sentenceService));
        }

        #region Основная статистика

        /// <summary>
        /// Получает общую сводку статистики изучения.
        /// </summary>
        public async Task<OverallStatistics> GetOverallStatisticsAsync()
        {
            await EnsureInitializedAsync();

            // Получаем базовые данные
            var allCards = await _databaseService.GetAllCardsAsync();
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var totalCards = allCards.Count;
            var learnedCards = allStats.Count(s => s.IsLearned);
            var cardsInProgress = allStats.Count(s => !s.IsLearned && s.RepetitionCount > 0);
            var cardsNotStarted = totalCards - learnedCards - cardsInProgress;
            var totalStudyTime = allStats.Sum(s => s.TotalStudyTimeSeconds);

            // Временные заглушки для остальных данных
            return new OverallStatistics
            {
                TotalCards = totalCards,
                CardsLearned = learnedCards,
                CardsInProgress = cardsInProgress,
                CardsNotStarted = cardsNotStarted,
                TotalStudyDays = 30,
                CurrentStreak = await GetCurrentStudyStreakAsync(),
                MaxStreak = await GetMaxStudyStreakAsync(),
                TotalStudyTime = TimeSpan.FromSeconds(totalStudyTime),
                OverallProgress = totalCards > 0 ? (double)learnedCards / totalCards * 100 : 0,
                AverageAccuracy = allStats.Count > 0 ? allStats.Average(s => s.AccuracyPercentage) : 0,
                FirstStudyDate = DateTime.UtcNow.AddDays(-60),
                LastStudyDate = DateTime.UtcNow.AddDays(-1),
                TotalStudySessions = 150,
                TotalCardsReviewed = allStats.Sum(s => s.RepetitionCount),
                TotalNewCardsLearned = learnedCards
            };
        }

        /// <summary>
        /// Получает статистику за сегодня.
        /// </summary>
        public async Task<DailyStatistics> GetTodayStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new DailyStatistics
            {
                Date = DateTime.UtcNow.Date,
                CardsStudied = 15,
                CardsReviewed = 25,
                NewCardsLearned = 5,
                CorrectAnswers = 35,
                TotalAnswers = 40,
                StudyTime = TimeSpan.FromMinutes(45),
                StudySessions = 3,
                AverageEFactor = 2.3,
                Streak = await GetCurrentStudyStreakAsync(),
                StudyGoalAchieved = true
            };
        }

        /// <summary>
        /// Получает статистику за указанный день.
        /// </summary>
        public async Task<DailyStatistics> GetStatisticsForDateAsync(DateTime date)
        {
            await EnsureInitializedAsync();

            // Временная заглушка - возвращаем данные для сегодняшнего дня
            return await GetTodayStatisticsAsync();
        }

        /// <summary>
        /// Получает статистику за период.
        /// </summary>
        public async Task<PeriodStatistics> GetStatisticsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();

            var totalDays = (endDate - startDate).Days + 1;

            return new PeriodStatistics
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                StudyDays = (int)(totalDays * 0.7), // 70% дней с изучением
                TotalCardsStudied = 500,
                TotalCardsReviewed = 1200,
                TotalNewCardsLearned = 150,
                TotalCorrectAnswers = 1000,
                TotalAnswers = 1200,
                TotalStudyTime = TimeSpan.FromHours(25),
                MaxStreak = 14,
                AverageEFactor = 2.2
            };
        }

        /// <summary>
        /// Получает текущую серию дней изучения подряд.
        /// </summary>
        public async Task<int> GetCurrentStudyStreakAsync()
        {
            await EnsureInitializedAsync();

            // Временная заглушка
            return 7;
        }

        /// <summary>
        /// Получает максимальную серию дней изучения подряд.
        /// </summary>
        public async Task<int> GetMaxStudyStreakAsync()
        {
            await EnsureInitializedAsync();

            // Временная заглушка
            return 21;
        }

        /// <summary>
        /// Получает общее время, потраченное на изучение.
        /// </summary>
        public async Task<TimeSpan> GetTotalStudyTimeAsync()
        {
            await EnsureInitializedAsync();

            var allStats = await _databaseService.GetAllSRSStatisticsAsync();
            var totalSeconds = allStats.Sum(s => s.TotalStudyTimeSeconds);
            return TimeSpan.FromSeconds(totalSeconds);
        }

        /// <summary>
        /// Получает среднее время изучения в день.
        /// </summary>
        public async Task<TimeSpan> GetAverageDailyStudyTimeAsync()
        {
            await EnsureInitializedAsync();

            var totalTime = await GetTotalStudyTimeAsync();
            var studyDays = 60; // Временная заглушка

            return studyDays > 0 ? TimeSpan.FromTicks(totalTime.Ticks / studyDays) : TimeSpan.Zero;
        }

        #endregion

        #region Статистика по времени

        /// <summary>
        /// Получает ежедневную статистику за указанный период.
        /// </summary>
        public async Task<List<DailyStatistics>> GetDailyStatisticsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();

            var dailyStats = new List<DailyStatistics>();
            var currentDate = startDate;
            var random = new Random();

            while (currentDate <= endDate)
            {
                dailyStats.Add(new DailyStatistics
                {
                    Date = currentDate,
                    CardsStudied = random.Next(5, 20),
                    CardsReviewed = random.Next(10, 30),
                    NewCardsLearned = random.Next(0, 8),
                    CorrectAnswers = random.Next(20, 40),
                    TotalAnswers = random.Next(25, 45),
                    StudyTime = TimeSpan.FromMinutes(random.Next(20, 90)),
                    StudySessions = random.Next(1, 5),
                    AverageEFactor = 2.0 + random.NextDouble() * 0.5,
                    Streak = random.Next(1, 15),
                    StudyGoalAchieved = random.Next(0, 2) == 1
                });

                currentDate = currentDate.AddDays(1);
            }

            return dailyStats;
        }

        /// <summary>
        /// Получает еженедельную статистику за указанный период.
        /// </summary>
        public async Task<List<WeeklyStatistics>> GetWeeklyStatisticsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();

            var weeklyStats = new List<WeeklyStatistics>();
            var currentWeekStart = startDate.AddDays(-(int)startDate.DayOfWeek);
            var random = new Random();
            var weekNumber = 1;

            while (currentWeekStart <= endDate)
            {
                var weekEnd = currentWeekStart.AddDays(6);

                weeklyStats.Add(new WeeklyStatistics
                {
                    WeekStart = currentWeekStart,
                    WeekEnd = weekEnd,
                    WeekNumber = weekNumber,
                    StudyDays = random.Next(3, 7),
                    CardsStudied = random.Next(50, 150),
                    CardsReviewed = random.Next(100, 300),
                    NewCardsLearned = random.Next(10, 40),
                    AccuracyRate = 70 + random.NextDouble() * 20,
                    StudyTime = TimeSpan.FromHours(random.Next(5, 20)),
                    ProgressChange = random.NextDouble() * 10 - 5 // от -5 до +5
                });

                currentWeekStart = currentWeekStart.AddDays(7);
                weekNumber++;
            }

            return weeklyStats;
        }

        /// <summary>
        /// Получает месячную статистику за указанный период.
        /// </summary>
        public async Task<List<MonthlyStatistics>> GetMonthlyStatisticsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();

            var monthlyStats = new List<MonthlyStatistics>();
            var currentDate = startDate;
            var random = new Random();

            while (currentDate <= endDate)
            {
                var monthName = currentDate.ToString("MMMM");

                monthlyStats.Add(new MonthlyStatistics
                {
                    Year = currentDate.Year,
                    Month = currentDate.Month,
                    MonthName = monthName,
                    StudyDays = random.Next(15, 25),
                    CardsStudied = random.Next(200, 500),
                    CardsReviewed = random.Next(400, 1000),
                    NewCardsLearned = random.Next(50, 150),
                    AccuracyRate = 75 + random.NextDouble() * 15,
                    StudyTime = TimeSpan.FromHours(random.Next(20, 60)),
                    ProgressChange = random.NextDouble() * 15 - 5, // от -5 до +10
                    BestStreak = random.Next(7, 21)
                });

                currentDate = currentDate.AddMonths(1);
            }

            return monthlyStats;
        }

        /// <summary>
        /// Получает статистику по времени суток (когда пользователь обычно изучает).
        /// </summary>
        public async Task<TimeOfDayStatistics> GetTimeOfDayStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new TimeOfDayStatistics
            {
                MorningSessions = 45,
                AfternoonSessions = 60,
                EveningSessions = 30,
                NightSessions = 15,
                MorningAccuracy = 85.5,
                AfternoonAccuracy = 82.3,
                EveningAccuracy = 78.9,
                NightAccuracy = 75.2,
                PreferredStudyTime = new TimeSpan(15, 30, 0) // 15:30
            };
        }

        /// <summary>
        /// Получает статистику по дням недели.
        /// </summary>
        public async Task<DayOfWeekStatistics> GetDayOfWeekStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new DayOfWeekStatistics
            {
                MondayAccuracy = 80.5,
                TuesdayAccuracy = 82.3,
                WednesdayAccuracy = 85.7,
                ThursdayAccuracy = 81.2,
                FridayAccuracy = 78.9,
                SaturdayAccuracy = 88.5,
                SundayAccuracy = 90.2,
                MondaySessions = 20,
                TuesdaySessions = 22,
                WednesdaySessions = 25,
                ThursdaySessions = 21,
                FridaySessions = 18,
                SaturdaySessions = 28,
                SundaySessions = 30,
                MostProductiveDay = "Воскресенье",
                LeastProductiveDay = "Пятница"
            };
        }

        #endregion

        #region Статистика по карточкам и изучению

        /// <summary>
        /// Получает статистику по карточкам (новые, изученные, для повторения).
        /// </summary>
        public async Task<CardStatistics> GetCardStatisticsAsync()
        {
            await EnsureInitializedAsync();

            var allCards = await _databaseService.GetAllCardsAsync();
            var allStats = await _databaseService.GetAllSRSStatisticsAsync();

            var totalCards = allCards.Count;
            var learnedCards = allStats.Count(s => s.IsLearned);
            var cardsToReview = allStats.Count(s => s.IsDueForReview());
            var cardsDueToday = allStats.Count(s => s.NextReviewDate.Date == DateTime.UtcNow.Date);
            var cardsDueTomorrow = allStats.Count(s => s.NextReviewDate.Date == DateTime.UtcNow.AddDays(1).Date);
            var newCardsAvailable = totalCards - allStats.Count;

            return new CardStatistics
            {
                TotalCards = totalCards,
                CardsLearned = learnedCards,
                CardsToReview = cardsToReview,
                CardsDueToday = cardsDueToday,
                CardsDueTomorrow = cardsDueTomorrow,
                NewCardsAvailable = newCardsAvailable,
                AverageEFactor = allStats.Count > 0 ? allStats.Average(s => s.EFactor) : 0,
                AverageEaseScore = allStats.Count > 0 ? allStats.Average(s => s.EaseScore) : 0
            };
        }

        /// <summary>
        /// Получает прогресс изучения карточек по уровням HSK.
        /// </summary>
        public async Task<List<HskLevelStatistics>> GetHskLevelStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new List<HskLevelStatistics>
            {
                new HskLevelStatistics
                {
                    HskLevel = 1,
                    TotalCards = 150,
                    CardsLearned = 120,
                    CardsInProgress = 20,
                    CardsNotStarted = 10,
                    AverageAccuracy = 88.5,
                    AverageEFactor = 2.3,
                    AverageStudyTime = TimeSpan.FromSeconds(15)
                },
                new HskLevelStatistics
                {
                    HskLevel = 2,
                    TotalCards = 300,
                    CardsLearned = 180,
                    CardsInProgress = 80,
                    CardsNotStarted = 40,
                    AverageAccuracy = 82.3,
                    AverageEFactor = 2.1,
                    AverageStudyTime = TimeSpan.FromSeconds(20)
                },
                new HskLevelStatistics
                {
                    HskLevel = 3,
                    TotalCards = 600,
                    CardsLearned = 200,
                    CardsInProgress = 150,
                    CardsNotStarted = 250,
                    AverageAccuracy = 75.8,
                    AverageEFactor = 1.9,
                    AverageStudyTime = TimeSpan.FromSeconds(25)
                }
            };
        }

        /// <summary>
        /// Получает статистику успеваемости по карточкам.
        /// </summary>
        public async Task<PerformanceStatistics> GetPerformanceStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new PerformanceStatistics
            {
                OverallAccuracy = 82.5,
                RecentAccuracy = 85.7,
                TrendAccuracy = 3.2, // улучшение на 3.2%
                BestAccuracyDay = 5,
                BestAccuracyRate = 95.8,
                BestAccuracyDate = DateTime.UtcNow.AddDays(-10),
                WorstAccuracyDay = 2,
                WorstAccuracyRate = 68.3,
                WorstAccuracyDate = DateTime.UtcNow.AddDays(-25),
                ConsistencyScore = 78.5,
                ImprovementRate = 2.1
            };
        }

        /// <summary>
        /// Получает статистику по типам иероглифов.
        /// </summary>
        public async Task<List<CharacterTypeStatistics>> GetCharacterTypeStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new List<CharacterTypeStatistics>
            {
                new CharacterTypeStatistics
                {
                    CharacterType = "Пиктограммы",
                    Description = "Иероглифы-изображения",
                    TotalCards = 100,
                    CardsLearned = 80,
                    AverageAccuracy = 90.5,
                    AverageDifficulty = 2.0,
                    AverageStudyTime = TimeSpan.FromSeconds(12)
                },
                new CharacterTypeStatistics
                {
                    CharacterType = "Фоноидеограммы",
                    Description = "Иероглифы с фонетиком и радикалом",
                    TotalCards = 500,
                    CardsLearned = 350,
                    AverageAccuracy = 82.3,
                    AverageDifficulty = 3.5,
                    AverageStudyTime = TimeSpan.FromSeconds(18)
                },
                new CharacterTypeStatistics
                {
                    CharacterType = "Идеограммы",
                    Description = "Составные иероглифы",
                    TotalCards = 150,
                    CardsLearned = 100,
                    AverageAccuracy = 78.9,
                    AverageDifficulty = 3.0,
                    AverageStudyTime = TimeSpan.FromSeconds(15)
                }
            };
        }

        /// <summary>
        /// Получает статистику по радикалам.
        /// </summary>
        public async Task<List<RadicalStatistics>> GetRadicalStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new List<RadicalStatistics>
            {
                new RadicalStatistics
                {
                    Radical = "人",
                    TotalCards = 50,
                    CardsLearned = 45,
                    AverageAccuracy = 92.5,
                    AverageDifficulty = 1.8,
                    RelatedCardsCount = 50
                },
                new RadicalStatistics
                {
                    Radical = "口",
                    TotalCards = 40,
                    CardsLearned = 35,
                    AverageAccuracy = 88.7,
                    AverageDifficulty = 2.2,
                    RelatedCardsCount = 40
                },
                new RadicalStatistics
                {
                    Radical = "水",
                    TotalCards = 35,
                    CardsLearned = 28,
                    AverageAccuracy = 85.3,
                    AverageDifficulty = 2.5,
                    RelatedCardsCount = 35
                }
            };
        }

        /// <summary>
        /// Получает статистику по сложности карточек.
        /// </summary>
        public async Task<DifficultyStatistics> GetDifficultyStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new DifficultyStatistics
            {
                EasyCards = 200,
                MediumCards = 350,
                HardCards = 150,
                VeryHardCards = 50,
                EasyCardsAccuracy = 92.5,
                MediumCardsAccuracy = 85.3,
                HardCardsAccuracy = 75.8,
                VeryHardCardsAccuracy = 62.4,
                AverageDifficultyScore = 2.8,
                MostDifficultCategory = "Фоноидеограммы"
            };
        }

        /// <summary>
        /// Получает самые сложные карточки для пользователя.
        /// </summary>
        public async Task<List<Card>> GetMostDifficultCardsAsync(int limit = 10)
        {
            await EnsureInitializedAsync();

            var allCards = await _databaseService.GetAllCardsAsync();
            return allCards.Take(limit).ToList();
        }

        /// <summary>
        /// Получает самые легкие карточки для пользователя.
        /// </summary>
        public async Task<List<Card>> GetEasiestCardsAsync(int limit = 10)
        {
            await EnsureInitializedAsync();

            var allCards = await _databaseService.GetAllCardsAsync();
            return allCards.Skip(Math.Max(0, allCards.Count - limit)).Take(limit).ToList();
        }

        #endregion

        #region Статистика по режимам обучения

        /// <summary>
        /// Получает статистику по режиму изучения карточек.
        /// </summary>
        public async Task<StudyModeStatistics> GetStudyModeStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new StudyModeStatistics
            {
                TotalSessions = 150,
                TotalCardsStudied = 2500,
                AverageAccuracy = 82.5,
                TotalTime = TimeSpan.FromHours(50),
                AverageSessionTime = TimeSpan.FromMinutes(20),
                BestSessionCards = 45,
                BestSessionDate = DateTime.UtcNow.AddDays(-15),
                BestSessionAccuracy = 95.8,
                LongestSessionMinutes = 75,
                LongestSessionDate = DateTime.UtcNow.AddDays(-30)
            };
        }

        /// <summary>
        /// Получает статистику по режиму диктанта.
        /// </summary>
        public async Task<DictationModeStatistics> GetDictationModeStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new DictationModeStatistics
            {
                TotalSessions = 45,
                TotalCharactersWritten = 1200,
                TotalCorrectCharacters = 980,
                TotalTime = TimeSpan.FromHours(15),
                BestSessionScore = 95,
                BestSessionDate = DateTime.UtcNow.AddDays(-10),
                MostPracticedCharacter = "学",
                MostPracticedCharacterCount = 85
            };
        }

        /// <summary>
        /// Получает статистику по режиму библиотеки иероглифов.
        /// </summary>
        public async Task<CharacterLibraryStatistics> GetCharacterLibraryStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new CharacterLibraryStatistics
            {
                TotalViews = 850,
                UniqueCharactersViewed = 350,
                SearchesPerformed = 120,
                MostSearchedTerm = "好",
                MostSearchedTermCount = 45,
                MostViewedCharacter = "人",
                MostViewedCharacterCount = 120,
                TotalTimeSpent = TimeSpan.FromHours(25)
            };
        }

        /// <summary>
        /// Получает статистику по режиму примеров предложений.
        /// </summary>
        public async Task<SentenceModeStatistics> GetSentenceModeStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new SentenceModeStatistics
            {
                TotalSentencesViewed = 450,
                TotalExercisesCompleted = 180,
                ExerciseAccuracy = 78.5,
                TotalTimeSpent = TimeSpan.FromHours(30),
                MostViewedSentence = "你好！",
                MostViewedSentenceCount = 85,
                SentencesTranslated = 120,
                TranslationAccuracy = 82.3
            };
        }

        #endregion

        #region Аналитика и рекомендации

        /// <summary>
        /// Получает рекомендации по улучшению процесса изучения.
        /// </summary>
        public async Task<List<StudyRecommendation>> GetStudyRecommendationsAsync()
        {
            await EnsureInitializedAsync();

            return new List<StudyRecommendation>
            {
                new StudyRecommendation
                {
                    Title = "Увеличьте постоянство",
                    Description = "Вы изучаете нерегулярно, что замедляет прогресс",
                    Category = "consistency",
                    Priority = 5,
                    Action = "Установите ежедневную цель и придерживайтесь её",
                    Reason = "Только 60% дней в месяце вы изучали карточки",
                    ExpectedBenefit = "Увеличение прогресса на 30%"
                },
                new StudyRecommendation
                {
                    Title = "Сфокусируйтесь на сложных карточках",
                    Description = "У вас много карточек с низкой точностью",
                    Category = "accuracy",
                    Priority = 4,
                    Action = "Повторите карточки с оценкой 1 и 2",
                    Reason = "25% карточек имеют точность ниже 70%",
                    ExpectedBenefit = "Увеличение общей точности на 15%"
                },
                new StudyRecommendation
                {
                    Title = "Разнообразьте методы обучения",
                    Description = "Вы используете только один режим обучения",
                    Category = "variety",
                    Priority = 3,
                    Action = "Попробуйте режим диктанта и примеры предложений",
                    Reason = "95% времени вы проводите только в режиме карточек",
                    ExpectedBenefit = "Улучшение запоминания на 20%"
                }
            };
        }

        /// <summary>
        /// Получает прогноз прогресса на основе текущей статистики.
        /// </summary>
        public async Task<ProgressForecast> GetProgressForecastAsync()
        {
            await EnsureInitializedAsync();

            return new ProgressForecast
            {
                ForecastDate = DateTime.UtcNow.AddMonths(3),
                EstimatedCardsLearned = 800,
                EstimatedProgress = 85.5,
                DaysToCompleteCurrentLevel = 90,
                EstimatedCompletionDate = DateTime.UtcNow.AddMonths(6),
                ConfidenceLevel = 0.8,
                Scenarios = new List<ForecastScenario>
                {
                    new ForecastScenario
                    {
                        Scenario = "optimistic",
                        EstimatedCardsLearned = 1000,
                        EstimatedProgress = 95.0,
                        EstimatedCompletionDate = DateTime.UtcNow.AddMonths(4)
                    },
                    new ForecastScenario
                    {
                        Scenario = "realistic",
                        EstimatedCardsLearned = 800,
                        EstimatedProgress = 85.5,
                        EstimatedCompletionDate = DateTime.UtcNow.AddMonths(6)
                    },
                    new ForecastScenario
                    {
                        Scenario = "pessimistic",
                        EstimatedCardsLearned = 600,
                        EstimatedProgress = 75.0,
                        EstimatedCompletionDate = DateTime.UtcNow.AddMonths(8)
                    }
                }
            };
        }

        /// <summary>
        /// Получает анализ слабых мест пользователя.
        /// </summary>
        public async Task<WeaknessAnalysis> GetWeaknessAnalysisAsync()
        {
            await EnsureInitializedAsync();

            return new WeaknessAnalysis
            {
                WeaknessAreas = new List<WeaknessArea>
                {
                    new WeaknessArea
                    {
                        Area = "hsk_level",
                        Name = "HSK 3",
                        WeaknessScore = 75.5,
                        Accuracy = 68.3,
                        TotalCards = 200,
                        CardsLearned = 80,
                        SpecificProblems = new List<string>
                        {
                            "Сложные иероглифы с большим количеством черт",
                            "Похожие по написанию иероглифы"
                        }
                    },
                    new WeaknessArea
                    {
                        Area = "character_type",
                        Name = "Фоноидеограммы",
                        WeaknessScore = 65.2,
                        Accuracy = 72.5,
                        TotalCards = 150,
                        CardsLearned = 90,
                        SpecificProblems = new List<string>
                        {
                            "Запоминание фонетических компонентов",
                            "Различение похожих радикалов"
                        }
                    }
                },
                PrimaryWeakness = "HSK 3",
                OverallWeaknessScore = 42.5,
                ImprovementSuggestions = new List<string>
                {
                    "Сфокусируйтесь на изучении карточек HSK 3",
                    "Используйте мнемонические приемы для сложных иероглифов",
                    "Практикуйте написание иероглифов в режиме диктанта"
                }
            };
        }

        /// <summary>
        /// Получает цели и достижения пользователя.
        /// </summary>
        public async Task<AchievementStatistics> GetAchievementStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new AchievementStatistics
            {
                TotalAchievements = 50,
                AchievementsUnlocked = 25,
                RecentAchievements = new List<Achievement>
                {
                    new Achievement
                    {
                        Name = "Неделя обучения",
                        Description = "Изучайте карточки 7 дней подряд",
                        Category = "consistency",
                        IsUnlocked = true,
                        UnlockDate = DateTime.UtcNow.AddDays(-1),
                        Points = 100,
                        Icon = "🏆",
                        Progress = 100
                    },
                    new Achievement
                    {
                        Name = "Мастер точности",
                        Description = "Достигните точности 90% в сессии",
                        Category = "accuracy",
                        IsUnlocked = true,
                        UnlockDate = DateTime.UtcNow.AddDays(-3),
                        Points = 150,
                        Icon = "🎯",
                        Progress = 100
                    }
                },
                NextAchievements = new List<Achievement>
                {
                    new Achievement
                    {
                        Name = "Месяц обучения",
                        Description = "Изучайте карточки 30 дней подряд",
                        Category = "consistency",
                        IsUnlocked = false,
                        UnlockDate = DateTime.MinValue,
                        Points = 500,
                        Icon = "🌟",
                        Progress = 65
                    },
                    new Achievement
                    {
                        Name = "Исследователь иероглифов",
                        Description = "Изучите 500 иероглифов",
                        Category = "progress",
                        IsUnlocked = false,
                        UnlockDate = DateTime.MinValue,
                        Points = 300,
                        Icon = "🔍",
                        Progress = 75
                    }
                },
                TotalPoints = 1250,
                Level = 5,
                PointsToNextLevel = 250
            };
        }

        /// <summary>
        /// Получает сравнение с другими пользователями (анонимное).
        /// </summary>
        public async Task<ComparisonStatistics> GetComparisonStatisticsAsync()
        {
            await EnsureInitializedAsync();

            return new ComparisonStatistics
            {
                PercentileAccuracy = 75.5,
                PercentileConsistency = 62.3,
                PercentileProgress = 68.7,
                RankTotalUsers = 10000,
                UserRank = 3125,
                ComparisonGroup = "intermediate",
                Metrics = new List<ComparisonMetric>
                {
                    new ComparisonMetric
                    {
                        Metric = "Точность",
                        UserValue = 82.5,
                        GroupAverage = 78.3,
                        GroupTop10Percent = 92.7,
                        Status = ComparisonStatus.AboveAverage
                    },
                    new ComparisonMetric
                    {
                        Metric = "Постоянство",
                        UserValue = 65.0,
                        GroupAverage = 70.2,
                        GroupTop10Percent = 90.5,
                        Status = ComparisonStatus.BelowAverage
                    },
                    new ComparisonMetric
                    {
                        Metric = "Прогресс",
                        UserValue = 55.5,
                        GroupAverage = 50.8,
                        GroupTop10Percent = 75.3,
                        Status = ComparisonStatus.AboveAverage
                    }
                }
            };
        }

        #endregion

        #region Экспорт и управление

        /// <summary>
        /// Экспортирует статистику в формате JSON.
        /// </summary>
        public async Task<string> ExportStatisticsToJsonAsync()
        {
            await EnsureInitializedAsync();

            // Временная заглушка
            return "{\"statistics\": \"JSON export will be implemented in future versions\"}";
        }

        /// <summary>
        /// Экспортирует статистику в формате CSV.
        /// </summary>
        public async Task<string> ExportStatisticsToCsvAsync()
        {
            await EnsureInitializedAsync();

            // Временная заглушка
            return "Date,CardsStudied,Accuracy\n2024-01-01,15,85.5\n2024-01-02,20,88.3";
        }

        /// <summary>
        /// Сбрасывает статистику (только для тестирования).
        /// </summary>
        public async Task ResetStatisticsAsync()
        {
            await EnsureInitializedAsync();

            // В реальной реализации здесь будет сброс статистики
            // Пока просто логируем
            Console.WriteLine("Statistics reset requested");
        }

        /// <summary>
        /// Инициализирует сервис статистики.
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

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Очищает кэш статистики.
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
