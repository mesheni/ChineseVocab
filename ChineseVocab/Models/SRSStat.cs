using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;
// using ChineseVocab.SRS; - временно закомментировано из-за ошибок компиляции

namespace ChineseVocab.Models
{
    /// <summary>
    /// Модель статистики системы интервальных повторений (SRS).
    /// Соответствует таблице SRSStatistics в базе данных.
    /// Реализует алгоритм SM-2 для расчета интервалов повторений.
    /// </summary>
    public partial class SRSStat : ObservableObject
    {
        /// <summary>
        /// Уникальный идентификатор записи статистики (первичный ключ).
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор карточки, к которой относится статистика.
        /// </summary>
        [Indexed]
        public int CardId { get; set; }

        /// <summary>
        /// Дата следующего повторения карточки.
        /// </summary>
        [ObservableProperty]
        private DateTime _nextReviewDate = DateTime.UtcNow;

        /// <summary>
        /// Текущий интервал повторения в днях.
        /// </summary>
        [ObservableProperty]
        private int _interval = 1;

        /// <summary>
        /// Фактор легкости (E-Factor) для алгоритма SM-2.
        /// Определяет, насколько быстро увеличивается интервал.
        /// Начальное значение 2.5, диапазон обычно 1.3 - 2.5.
        /// </summary>
        [ObservableProperty]
        private double _eFactor = 2.5;

        /// <summary>
        /// Количество повторений карточки (сколько раз она была успешно повторена).
        /// </summary>
        [ObservableProperty]
        private int _repetitionCount = 0;

        /// <summary>
        /// Оценка легкости (Ease Score) - пользовательская оценка от 0 до 5.
        /// </summary>
        [ObservableProperty]
        private int _easeScore = 3;

        /// <summary>
        /// Дата и время последнего повторения карточки.
        /// </summary>
        [ObservableProperty]
        private DateTime _lastReviewed = DateTime.MinValue;

        /// <summary>
        /// Текущая серия правильных ответов подряд.
        /// </summary>
        [ObservableProperty]
        private int _correctStreak = 0;

        /// <summary>
        /// Общее количество правильных ответов для этой карточки.
        /// </summary>
        [ObservableProperty]
        private int _totalCorrect = 0;

        /// <summary>
        /// Общее количество неправильных ответов для этой карточки.
        /// </summary>
        [ObservableProperty]
        private int _totalIncorrect = 0;

        /// <summary>
        /// Процент правильных ответов.
        /// </summary>
        public double AccuracyPercentage => (TotalCorrect + TotalIncorrect) > 0
            ? (double)TotalCorrect / (TotalCorrect + TotalIncorrect) * 100.0
            : 0.0;

        /// <summary>
        /// Общее время, потраченное на изучение этой карточки (в секундах).
        /// </summary>
        [ObservableProperty]
        private int _totalStudyTimeSeconds = 0;

        /// <summary>
        /// Среднее время ответа на карточку (в секундах).
        /// </summary>
        public double AverageResponseTime => RepetitionCount > 0
            ? (double)TotalStudyTimeSeconds / RepetitionCount
            : 0.0;

        /// <summary>
        /// Признак, что карточка выучена (интервал превышает определенный порог).
        /// </summary>
        [ObservableProperty]
        private bool _isLearned = false;

        /// <summary>
        /// Дата, когда карточка была выучена (если IsLearned = true).
        /// </summary>
        [ObservableProperty]
        private DateTime _learnedDate = DateTime.MinValue;

        /// <summary>
        /// Уровень уверенности пользователя в карточке (1-5).
        /// </summary>
        [ObservableProperty]
        private int _confidenceLevel = 1;

        /// <summary>
        /// Дополнительные заметки по изучению этой карточки.
        /// </summary>
        [ObservableProperty]
        private string _notes = string.Empty;

        /// <summary>
        /// Дата создания записи статистики.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего изменения записи статистики.
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public SRSStat() { }

        /// <summary>
        /// Конструктор для создания статистики для новой карточки.
        /// </summary>
        public SRSStat(int cardId)
        {
            CardId = cardId;
            NextReviewDate = DateTime.UtcNow.Date; // Первое повторение сегодня
            Interval = ChineseVocab.SRS.SRSEngine.FirstReviewInterval;
            EFactor = ChineseVocab.SRS.SRSEngine.DefaultEFactor;
            RepetitionCount = 0;
            EaseScore = 3;
            LastReviewed = DateTime.MinValue;
            CorrectStreak = 0;
        }

        /// <summary>
        /// Применяет алгоритм SM-2 для обработки оценки пользователя.
        /// </summary>
        /// <param name="quality">Оценка качества ответа от 0 до 5, где:
        /// 0 - полный провал
        /// 1 - неправильный ответ
        /// 2 - почти правильно
        /// 3 - правильно с затруднениями
        /// 4 - правильно легко
        /// 5 - правильно мгновенно
        /// </param>
        public void ProcessSM2Review(int quality)
        {
            // Ограничиваем качество в диапазоне 0-5
            quality = Math.Clamp(quality, 0, 5);

            // Обновляем статистику
            LastReviewed = DateTime.UtcNow;
            EaseScore = quality;

            // Используем SRSEngine для обработки повторения
            var (newInterval, newRepetitionCount, newEFactor) =
                ChineseVocab.SRS.SRSEngine.ProcessReview(Interval, RepetitionCount, EFactor, quality);

            if (quality < ChineseVocab.SRS.SRSEngine.MinimumPassingQuality)
            {
                // Неправильный или плохой ответ - сбрасываем прогресс
                RepetitionCount = 0;
                CorrectStreak = 0;
                TotalIncorrect++;
            }
            else
            {
                // Правильный ответ
                RepetitionCount = newRepetitionCount;
                CorrectStreak++;
                TotalCorrect++;
            }

            // Обновляем интервал и фактор легкости из результата SRSEngine
            Interval = newInterval;
            EFactor = newEFactor;

            // Рассчитываем следующую дату повторения
            NextReviewDate = DateTime.UtcNow.AddDays(Interval);

            // Проверяем, выучена ли карточка
            if (ChineseVocab.SRS.SRSEngine.IsCardLearned(Interval) && !IsLearned)
            {
                IsLearned = true;
                LearnedDate = DateTime.UtcNow;
            }

            // Обновляем счетчик времени (предполагаем среднее время 10 секунд за повторение)
            TotalStudyTimeSeconds += 10;

            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Проверяет, нужно ли повторять карточку сегодня.
        /// </summary>
        public bool IsDueForReview()
        {
            return DateTime.UtcNow.Date >= NextReviewDate.Date;
        }

        /// <summary>
        /// Проверяет, просрочена ли карточка для повторения.
        /// </summary>
        public bool IsOverdue()
        {
            return DateTime.UtcNow.Date > NextReviewDate.Date;
        }

        /// <summary>
        /// Возвращает количество дней до следующего повторения.
        /// Отрицательное значение означает просрочку.
        /// </summary>
        public int DaysUntilNextReview()
        {
            return (NextReviewDate.Date - DateTime.UtcNow.Date).Days;
        }

        /// <summary>
        /// Сбрасывает статистику карточки (например, после длительного перерыва).
        /// </summary>
        public void Reset()
        {
            Interval = ChineseVocab.SRS.SRSEngine.FirstReviewInterval;
            RepetitionCount = 0;
            CorrectStreak = 0;
            NextReviewDate = DateTime.UtcNow.Date;
            IsLearned = false;
            LearnedDate = DateTime.MinValue;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Возвращает строковое представление статистики (для отладки).
        /// </summary>
        public override string ToString()
        {
            return $"Карточка {CardId}: след. повторение {NextReviewDate:yyyy-MM-dd}, интервал {Interval} дн., E-Factor {EFactor:F2}, повторений {RepetitionCount}";
        }
    }
}
