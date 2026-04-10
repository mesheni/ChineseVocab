using System;
using Xunit;
using ChineseVocab.Models;

namespace ChineseVocab.Tests
{
    public class SRSStatTests
    {
        [Fact]
        public void Constructor_Default_CreatesEmptyStat()
        {
            // Arrange & Act
            var stat = new SRSStat();

            // Assert
            Assert.Equal(0, stat.Id);
            Assert.Equal(0, stat.CardId);
            Assert.Equal(default(DateTime), stat.NextReviewDate);
            Assert.Equal(1, stat.Interval);
            Assert.Equal(2.5, stat.EFactor);
            Assert.Equal(0, stat.RepetitionCount);
            Assert.Equal(3, stat.EaseScore);
            Assert.Equal(DateTime.MinValue, stat.LastReviewed);
            Assert.Equal(0, stat.CorrectStreak);
        }

        [Fact]
        public void Constructor_WithCardId_CreatesInitialStatForCard()
        {
            // Arrange
            int cardId = 123;

            // Act
            var stat = new SRSStat(cardId);

            // Assert
            Assert.Equal(cardId, stat.CardId);
            Assert.Equal(DateTime.UtcNow.Date, stat.NextReviewDate.Date); // Первое повторение сегодня
            Assert.Equal(1, stat.Interval);
            Assert.Equal(2.5, stat.EFactor);
            Assert.Equal(0, stat.RepetitionCount);
            Assert.Equal(3, stat.EaseScore);
            Assert.Equal(DateTime.MinValue, stat.LastReviewed);
            Assert.Equal(0, stat.CorrectStreak);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(4)]
        [InlineData(3)]
        public void ProcessSM2Review_CorrectAnswer_IncrementsStats(int quality)
        {
            // Arrange
            var stat = new SRSStat(123);
            int initialRepetitionCount = stat.RepetitionCount;
            int initialCorrectStreak = stat.CorrectStreak;
            int initialTotalCorrect = stat.TotalCorrect;
            double initialEFactor = stat.EFactor;
            int initialInterval = stat.Interval;

            // Act
            stat.ProcessSM2Review(quality);

            // Assert
            Assert.Equal(quality, stat.EaseScore);
            Assert.True(stat.LastReviewed > DateTime.MinValue);
            Assert.Equal(initialRepetitionCount + 1, stat.RepetitionCount);
            Assert.Equal(initialCorrectStreak + 1, stat.CorrectStreak);
            Assert.Equal(initialTotalCorrect + 1, stat.TotalCorrect);
            Assert.True(stat.NextReviewDate > DateTime.UtcNow);

            // Проверяем изменения в зависимости от качества
            if (quality > 3)
            {
                Assert.True(stat.EFactor > initialEFactor, "E-Factor should increase for high quality");
            }
            else if (quality == 3)
            {
                // Для качества 3 E-Factor может остаться таким же или немного уменьшиться
                Assert.True(stat.EFactor <= initialEFactor, "E-Factor should not increase for quality 3");
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(1)]
        [InlineData(0)]
        public void ProcessSM2Review_IncorrectAnswer_ResetsProgress(int quality)
        {
            // Arrange
            var stat = new SRSStat(123);

            // Сначала правильный ответ для установки прогресса
            stat.ProcessSM2Review(4);
            int initialTotalIncorrect = stat.TotalIncorrect;

            // Act
            stat.ProcessSM2Review(quality);

            // Assert
            Assert.Equal(quality, stat.EaseScore);
            Assert.True(stat.LastReviewed > DateTime.MinValue);
            Assert.Equal(0, stat.RepetitionCount); // Сброс счетчика повторений
            Assert.Equal(0, stat.CorrectStreak); // Сброс серии правильных ответов
            Assert.Equal(initialTotalIncorrect + 1, stat.TotalIncorrect);
            Assert.Equal(1, stat.Interval); // Сброс интервала до 1 дня
            Assert.True(stat.EFactor < 2.5, "E-Factor should decrease for incorrect answer");
        }

        [Fact]
        public void ProcessSM2Review_MultipleCorrectAnswers_IncreasesIntervalProgressively()
        {
            // Arrange
            var stat = new SRSStat(123);

            // Act - серия правильных ответов
            stat.ProcessSM2Review(4); // Первое повторение
            int interval1 = stat.Interval;

            stat.NextReviewDate = DateTime.UtcNow; // Имитируем наступление даты повторения
            stat.ProcessSM2Review(4); // Второе повторение
            int interval2 = stat.Interval;

            stat.NextReviewDate = DateTime.UtcNow;
            stat.ProcessSM2Review(4); // Третье повторение
            int interval3 = stat.Interval;

            // Assert
            Assert.True(interval2 > interval1, "Second interval should be greater than first");
            Assert.True(interval3 > interval2, "Third interval should be greater than second");
        }

        [Fact]
        public void ProcessSM2Review_HighQualitySequence_MarksCardAsLearned()
        {
            // Arrange
            var stat = new SRSStat(123);

            // Act - имитируем достаточно повторений для изучения карточки
            // Будем использовать высокие оценки для быстрого увеличения интервала
            for (int i = 0; i < 5; i++)
            {
                stat.ProcessSM2Review(5);
                stat.NextReviewDate = DateTime.UtcNow; // Сразу наступает следующая дата
            }

            // Assert
            Assert.True(stat.IsLearned, "Card should be marked as learned");
            Assert.True(stat.LearnedDate > DateTime.MinValue, "Learned date should be set");
            Assert.True(stat.Interval >= 21, "Interval should be at least 21 days for learned card");
        }

        [Fact]
        public void IsDueForReview_DateInPast_ReturnsTrue()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.AddDays(-1) // Вчера
            };

            // Act & Assert
            Assert.True(stat.IsDueForReview());
        }

        [Fact]
        public void IsDueForReview_DateToday_ReturnsTrue()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.Date // Сегодня
            };

            // Act & Assert
            Assert.True(stat.IsDueForReview());
        }

        [Fact]
        public void IsDueForReview_DateInFuture_ReturnsFalse()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.AddDays(1) // Завтра
            };

            // Act & Assert
            Assert.False(stat.IsDueForReview());
        }

        [Fact]
        public void IsOverdue_DateInPast_ReturnsTrue()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.AddDays(-2) // Позавчера
            };

            // Act & Assert
            Assert.True(stat.IsOverdue());
        }

        [Fact]
        public void IsOverdue_DateToday_ReturnsFalse()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.Date // Сегодня
            };

            // Act & Assert
            Assert.False(stat.IsOverdue());
        }

        [Fact]
        public void DaysUntilNextReview_FutureDate_ReturnsPositive()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.AddDays(5).Date
            };

            // Act
            int days = stat.DaysUntilNextReview();

            // Assert
            Assert.Equal(5, days);
        }

        [Fact]
        public void DaysUntilNextReview_PastDate_ReturnsNegative()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.AddDays(-3).Date
            };

            // Act
            int days = stat.DaysUntilNextReview();

            // Assert
            Assert.Equal(-3, days);
        }

        [Fact]
        public void DaysUntilNextReview_Today_ReturnsZero()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = DateTime.UtcNow.Date
            };

            // Act
            int days = stat.DaysUntilNextReview();

            // Assert
            Assert.Equal(0, days);
        }

        [Fact]
        public void Reset_ClearsProgress()
        {
            // Arrange
            var stat = new SRSStat(123);

            // Устанавливаем некоторый прогресс
            stat.ProcessSM2Review(4);
            stat.ProcessSM2Review(4);
            Assert.True(stat.RepetitionCount > 0);
            Assert.True(stat.CorrectStreak > 0);
            Assert.True(stat.Interval > 1);

            // Act
            stat.Reset();

            // Assert
            Assert.Equal(1, stat.Interval);
            Assert.Equal(0, stat.RepetitionCount);
            Assert.Equal(0, stat.CorrectStreak);
            Assert.Equal(DateTime.UtcNow.Date, stat.NextReviewDate.Date);
            Assert.False(stat.IsLearned);
            Assert.Equal(DateTime.MinValue, stat.LearnedDate);
            Assert.True(stat.ModifiedDate > DateTime.MinValue);
        }

        [Fact]
        public void AccuracyPercentage_NoReviews_ReturnsZero()
        {
            // Arrange
            var stat = new SRSStat(123);

            // Act & Assert
            Assert.Equal(0, stat.AccuracyPercentage);
        }

        [Fact]
        public void AccuracyPercentage_WithReviews_ReturnsCorrectValue()
        {
            // Arrange
            var stat = new SRSStat(123);

            // 3 правильных, 1 неправильный
            stat.ProcessSM2Review(4);
            stat.ProcessSM2Review(4);
            stat.ProcessSM2Review(4);
            stat.ProcessSM2Review(2);

            // Act
            double accuracy = stat.AccuracyPercentage;

            // Assert
            Assert.Equal(75.0, accuracy); // 3 из 4 = 75%
        }

        [Fact]
        public void AverageResponseTime_NoReviews_ReturnsZero()
        {
            // Arrange
            var stat = new SRSStat(123);

            // Act & Assert
            Assert.Equal(0, stat.AverageResponseTime);
        }

        [Fact]
        public void AverageResponseTime_WithReviews_ReturnsAverage()
        {
            // Arrange
            var stat = new SRSStat(123);

            // Имитируем несколько повторений
            stat.ProcessSM2Review(4);
            stat.ProcessSM2Review(4);
            stat.ProcessSM2Review(4);

            // Act
            double averageTime = stat.AverageResponseTime;

            // Assert
            Assert.True(averageTime > 0);
            // По умолчанию ProcessSM2Review добавляет 10 секунд на повторение
            Assert.Equal(10.0, averageTime);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var stat = new SRSStat(123)
            {
                NextReviewDate = new DateTime(2024, 1, 15),
                Interval = 7,
                EFactor = 2.3,
                RepetitionCount = 3
            };

            // Act
            string result = stat.ToString();

            // Assert
            Assert.Contains("Карточка 123", result);
            Assert.Contains("2024-01-15", result);
            Assert.Contains("7", result);
            Assert.Contains("2.3", result);
            Assert.Contains("3", result);
        }

        [Fact]
        public void ConfidenceLevel_CanBeSetAndRetrieved()
        {
            // Arrange
            var stat = new SRSStat(123);
            int expectedConfidence = 4;

            // Act
            stat.ConfidenceLevel = expectedConfidence;

            // Assert
            Assert.Equal(expectedConfidence, stat.ConfidenceLevel);
        }

        [Fact]
        public void Notes_CanBeSetAndRetrieved()
        {
            // Arrange
            var stat = new SRSStat(123);
            string expectedNotes = "Трудный иероглиф, нужно больше практики";

            // Act
            stat.Notes = expectedNotes;

            // Assert
            Assert.Equal(expectedNotes, stat.Notes);
        }

        [Fact]
        public void CreatedDate_SetOnCreation()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var stat = new SRSStat(123);

            // Assert
            Assert.True(stat.CreatedDate > beforeCreation);
            Assert.True(stat.CreatedDate <= DateTime.UtcNow);
        }

        [Fact]
        public void ModifiedDate_UpdatedOnProcessReview()
        {
            // Arrange
            var stat = new SRSStat(123);
            var initialModifiedDate = stat.ModifiedDate;

            // Act
            stat.ProcessSM2Review(4);

            // Assert
            Assert.True(stat.ModifiedDate > initialModifiedDate);
        }
    }
}
