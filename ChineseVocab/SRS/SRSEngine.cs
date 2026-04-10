using System;

namespace ChineseVocab.SRS
{
    /// <summary>
    /// Статический класс, реализующий алгоритм SM-2 для системы интервальных повторений (SRS).
    /// Используется для расчета интервалов повторения, фактора легкости и следующей даты повторения.
    /// </summary>
    public static class SRSEngine
    {
        #region Константы SM-2

        /// <summary>
        /// Начальное значение фактора легкости (E-Factor).
        /// </summary>
        public const double DefaultEFactor = 2.5;

        /// <summary>
        /// Минимальное значение фактора легкости.
        /// </summary>
        public const double MinEFactor = 1.3;

        /// <summary>
        /// Максимальное значение фактора легкости.
        /// </summary>
        public const double MaxEFactor = 2.5;

        /// <summary>
        /// Интервал первого повторения (в днях).
        /// </summary>
        public const int FirstReviewInterval = 1;

        /// <summary>
        /// Интервал второго повторения (в днях).
        /// </summary>
        public const int SecondReviewInterval = 6;

        /// <summary>
        /// Пороговое значение интервала для пометки карточки как выученной (в днях).
        /// </summary>
        public const int LearnedThreshold = 21;

        /// <summary>
        /// Минимальная оценка качества для засчитывания правильного ответа.
        /// </summary>
        public const int MinimumPassingQuality = 3;

        #endregion

        #region Основные методы SM-2

        /// <summary>
        /// Рассчитывает новый фактор легкости (E-Factor) на основе текущего E-Factor и оценки качества.
        /// Формула: EF' = EF + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02))
        /// </summary>
        /// <param name="currentEFactor">Текущий фактор легкости.</param>
        /// <param name="quality">Оценка качества от 0 до 5.</param>
        /// <returns>Новый фактор легкости, ограниченный в диапазоне [MinEFactor, MaxEFactor].</returns>
        public static double CalculateNewEFactor(double currentEFactor, int quality)
        {
            // Ограничиваем оценку качества
            quality = Math.Clamp(quality, 0, 5);

            // Формула SM-2 для расчета нового E-Factor
            double newEFactor = currentEFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));

            // Ограничиваем значение в допустимом диапазоне
            newEFactor = Math.Max(MinEFactor, Math.Min(MaxEFactor, newEFactor));

            return newEFactor;
        }

        /// <summary>
        /// Рассчитывает следующий интервал повторения на основе текущих параметров.
        /// </summary>
        /// <param name="currentInterval">Текущий интервал в днях.</param>
        /// <param name="repetitionCount">Количество успешных повторений.</param>
        /// <param name="eFactor">Текущий фактор легкости.</param>
        /// <param name="quality">Оценка качества от 0 до 5.</param>
        /// <returns>Новый интервал в днях.</returns>
        public static int CalculateNextInterval(int currentInterval, int repetitionCount, double eFactor, int quality)
        {
            // Ограничиваем оценку качества
            quality = Math.Clamp(quality, 0, 5);

            // Если оценка ниже минимальной проходной, сбрасываем интервал
            if (quality < MinimumPassingQuality)
            {
                return 1; // Сброс до первого интервала
            }

            // Определяем интервал по количеству повторений
            if (repetitionCount == 0)
            {
                // Первое повторение
                return FirstReviewInterval;
            }
            else if (repetitionCount == 1)
            {
                // Второе повторение
                return SecondReviewInterval;
            }
            else
            {
                // Последующие повторения: I(n) = I(n-1) * EF
                return (int)Math.Round(currentInterval * eFactor);
            }
        }

        /// <summary>
        /// Рассчитывает следующую дату повторения на основе текущих параметров.
        /// </summary>
        /// <param name="currentInterval">Текущий интервал в днях.</param>
        /// <param name="repetitionCount">Количество успешных повторений.</param>
        /// <param name="eFactor">Текущий фактор легкости.</param>
        /// <param name="quality">Оценка качества от 0 до 5.</param>
        /// <param name="referenceDate">Дата, от которой рассчитывается следующий интервал (обычно текущая дата).</param>
        /// <returns>Дата следующего повторения.</returns>
        public static DateTime CalculateNextReviewDate(int currentInterval, int repetitionCount, double eFactor, int quality, DateTime referenceDate)
        {
            int nextInterval = CalculateNextInterval(currentInterval, repetitionCount, eFactor, quality);
            return referenceDate.AddDays(nextInterval);
        }

        /// <summary>
        /// Обрабатывает полный цикл повторения по алгоритму SM-2.
        /// </summary>
        /// <param name="currentInterval">Текущий интервал в днях.</param>
        /// <param name="currentRepetitionCount">Текущее количество повторений.</param>
        /// <param name="currentEFactor">Текущий фактор легкости.</param>
        /// <param name="quality">Оценка качества от 0 до 5.</param>
        /// <returns>Кортеж с новыми значениями: (новый интервал, новое количество повторений, новый фактор легкости).</returns>
        public static (int NewInterval, int NewRepetitionCount, double NewEFactor) ProcessReview(int currentInterval, int currentRepetitionCount, double currentEFactor, int quality)
        {
            // Ограничиваем оценку качества
            quality = Math.Clamp(quality, 0, 5);

            // Рассчитываем новые значения
            double newEFactor = CalculateNewEFactor(currentEFactor, quality);

            int newRepetitionCount;
            int newInterval;

            if (quality < MinimumPassingQuality)
            {
                // Неправильный ответ - сбрасываем прогресс
                newRepetitionCount = 0;
                newInterval = 1;
            }
            else
            {
                // Правильный ответ
                newRepetitionCount = currentRepetitionCount + 1;
                newInterval = CalculateNextInterval(currentInterval, newRepetitionCount, newEFactor, quality);
            }

            return (newInterval, newRepetitionCount, newEFactor);
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет, является ли оценка качества допустимой (в диапазоне 0-5).
        /// </summary>
        public static bool IsValidQualityScore(int quality)
        {
            return quality >= 0 && quality <= 5;
        }

        /// <summary>
        /// Преобразует оценку качества в текстовое описание.
        /// </summary>
        public static string QualityScoreToDescription(int quality)
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
        /// Проверяет, можно ли считать карточку выученной на основе текущего интервала.
        /// </summary>
        public static bool IsCardLearned(int currentInterval)
        {
            return currentInterval >= LearnedThreshold;
        }

        /// <summary>
        /// Рассчитывает прогнозируемую дату, когда карточка будет считаться выученной.
        /// </summary>
        public static DateTime? CalculateProjectedLearnDate(DateTime startDate, int currentInterval, int currentRepetitionCount, double currentEFactor, int estimatedFutureQuality = 4)
        {
            // Используем симуляцию для прогнозирования
            DateTime currentDate = startDate;
            int interval = currentInterval;
            int repetitionCount = currentRepetitionCount;
            double eFactor = currentEFactor;

            // Максимальное количество итераций для предотвращения бесконечного цикла
            const int maxIterations = 50;
            int iterations = 0;

            while (!IsCardLearned(interval) && iterations < maxIterations)
            {
                var result = ProcessReview(interval, repetitionCount, eFactor, estimatedFutureQuality);
                interval = result.NewInterval;
                repetitionCount = result.NewRepetitionCount;
                eFactor = result.NewEFactor;

                // Пропускаем дни для следующего повторения
                currentDate = currentDate.AddDays(interval);
                iterations++;
            }

            return IsCardLearned(interval) ? currentDate : null;
        }

        /// <summary>
        /// Рассчитывает фактор забывания для карточки на основе текущего E-Factor и времени с последнего повторения.
        /// </summary>
        public static double CalculateForgettingFactor(double eFactor, TimeSpan timeSinceLastReview)
        {
            // Эвристическая формула: чем больше времени прошло и ниже E-Factor, тем выше фактор забывания
            double daysSinceReview = timeSinceLastReview.TotalDays;

            // Базовый фактор забывания от 0 (не забыл) до 1 (полностью забыл)
            double forgettingFactor = 1.0 - Math.Exp(-daysSinceReview / (eFactor * 10));

            return Math.Clamp(forgettingFactor, 0, 1);
        }

        /// <summary>
        /// Корректирует интервал повторения для долгого перерыва в изучении.
        /// </summary>
        public static int AdjustIntervalForLongBreak(int currentInterval, TimeSpan breakDuration)
        {
            if (breakDuration.TotalDays <= currentInterval * 2)
            {
                // Короткий перерыв, оставляем интервал как есть
                return currentInterval;
            }

            // Длинный перерыв - уменьшаем интервал пропорционально
            double reductionFactor = Math.Max(0.1, 1.0 - (breakDuration.TotalDays / (currentInterval * 10)));
            return (int)Math.Max(1, Math.Round(currentInterval * reductionFactor));
        }

        #endregion

        #region Методы для работы со статистикой

        /// <summary>
        /// Создает начальную статистику SRS для новой карточки.
        /// </summary>
        public static (int Interval, int RepetitionCount, double EFactor) CreateInitialStats()
        {
            return (FirstReviewInterval, 0, DefaultEFactor);
        }

        /// <summary>
        /// Рассчитывает рейтинг уверенности в карточке на основе статистики.
        /// </summary>
        public static double CalculateConfidenceScore(int correctStreak, int totalCorrect, int totalIncorrect, double eFactor)
        {
            if (totalCorrect + totalIncorrect == 0)
            {
                return 0;
            }

            double accuracy = (double)totalCorrect / (totalCorrect + totalIncorrect);
            double streakBonus = Math.Min(correctStreak / 10.0, 1.0); // Бонус за серию правильных ответов
            double eFactorBonus = (eFactor - MinEFactor) / (MaxEFactor - MinEFactor); // Нормализованный E-Factor

            // Комбинированный рейтинг уверенности (0-1)
            return (accuracy * 0.5) + (streakBonus * 0.3) + (eFactorBonus * 0.2);
        }

        /// <summary>
        /// Рассчитывает приоритет повторения карточки на основе статистики.
        /// </summary>
        public static double CalculateReviewPriority(DateTime nextReviewDate, double eFactor, int correctStreak, int daysOverdue = 0)
        {
            // Базовый приоритет - сколько дней до/после даты повторения
            double basePriority = daysOverdue > 0 ?
                daysOverdue * 2.0 : // Просроченные карточки имеют более высокий приоритет
                -(nextReviewDate - DateTime.UtcNow).TotalDays; // Чем раньше дата, тем выше приоритет

            // Корректировка на основе E-Factor
            double eFactorAdjustment = (MaxEFactor - eFactor) / (MaxEFactor - MinEFactor); // Низкий E-Factor увеличивает приоритет

            // Корректировка на основе серии правильных ответов
            double streakAdjustment = Math.Max(0, 1.0 - (correctStreak / 20.0)); // Длинные серии уменьшают приоритет

            return basePriority + eFactorAdjustment + streakAdjustment;
        }

        #endregion
    }
}
