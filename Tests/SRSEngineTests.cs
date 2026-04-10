using System;
using Xunit;

namespace ChineseVocab.Tests
{
    public class SRSEngineTests
    {
        [Fact]
        public void CalculateNewEFactor_ValidInput_ReturnsCorrectValue()
        {
            // Arrange
            double initialEFactor = 2.5;
            int quality = 4;

            // Act
            double result = ChineseVocab.SRS.SRSEngine.CalculateNewEFactor(initialEFactor, quality);

            // Assert
            Assert.InRange(result, ChineseVocab.SRS.SRSEngine.MinEFactor, ChineseVocab.SRS.SRSEngine.MaxEFactor);
            Assert.True(result > initialEFactor, "E-Factor should increase for quality 4");
        }

        [Fact]
        public void CalculateNewEFactor_LowQuality_DecreasesEFactor()
        {
            // Arrange
            double initialEFactor = 2.5;
            int quality = 2;

            // Act
            double result = ChineseVocab.SRS.SRSEngine.CalculateNewEFactor(initialEFactor, quality);

            // Assert
            Assert.InRange(result, ChineseVocab.SRS.SRSEngine.MinEFactor, ChineseVocab.SRS.SRSEngine.MaxEFactor);
            Assert.True(result < initialEFactor, "E-Factor should decrease for quality 2");
        }

        [Fact]
        public void CalculateNewEFactor_MinimumQuality_ReturnsMinEFactor()
        {
            // Arrange
            double initialEFactor = 2.5;
            int quality = 0;

            // Act
            double result = ChineseVocab.SRS.SRSEngine.CalculateNewEFactor(initialEFactor, quality);

            // Assert
            Assert.Equal(ChineseVocab.SRS.SRSEngine.MinEFactor, result);
        }

        [Fact]
        public void CalculateNextInterval_FirstReview_ReturnsFirstInterval()
        {
            // Arrange
            int currentInterval = 1;
            int repetitionCount = 0;
            double eFactor = 2.5;
            int quality = 4;

            // Act
            int result = ChineseVocab.SRS.SRSEngine.CalculateNextInterval(currentInterval, repetitionCount, eFactor, quality);

            // Assert
            Assert.Equal(ChineseVocab.SRS.SRSEngine.FirstReviewInterval, result);
        }

        [Fact]
        public void CalculateNextInterval_SecondReview_ReturnsSecondInterval()
        {
            // Arrange
            int currentInterval = 1;
            int repetitionCount = 1;
            double eFactor = 2.5;
            int quality = 4;

            // Act
            int result = ChineseVocab.SRS.SRSEngine.CalculateNextInterval(currentInterval, repetitionCount, eFactor, quality);

            // Assert
            Assert.Equal(ChineseVocab.SRS.SRSEngine.SecondReviewInterval, result);
        }

        [Fact]
        public void CalculateNextInterval_ThirdReview_ReturnsIncreasedInterval()
        {
            // Arrange
            int currentInterval = 6;
            int repetitionCount = 2;
            double eFactor = 2.5;
            int quality = 4;

            // Act
            int result = ChineseVocab.SRS.SRSEngine.CalculateNextInterval(currentInterval, repetitionCount, eFactor, quality);

            // Assert
            Assert.True(result > currentInterval, "Interval should increase for third review");
            Assert.Equal(15, result); // 6 * 2.5 = 15
        }

        [Fact]
        public void CalculateNextInterval_LowQuality_ResetsToOneDay()
        {
            // Arrange
            int currentInterval = 30;
            int repetitionCount = 5;
            double eFactor = 2.2;
            int quality = 2; // Below minimum passing quality

            // Act
            int result = ChineseVocab.SRS.SRSEngine.CalculateNextInterval(currentInterval, repetitionCount, eFactor, quality);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void ProcessReview_CorrectAnswer_IncreasesRepetitionCount()
        {
            // Arrange
            int currentInterval = 1;
            int currentRepetitionCount = 0;
            double currentEFactor = 2.5;
            int quality = 4;

            // Act
            var (newInterval, newRepetitionCount, newEFactor) =
                ChineseVocab.SRS.SRSEngine.ProcessReview(currentInterval, currentRepetitionCount, currentEFactor, quality);

            // Assert
            Assert.Equal(currentRepetitionCount + 1, newRepetitionCount);
            Assert.True(newInterval > currentInterval);
            Assert.True(newEFactor > currentEFactor);
        }

        [Fact]
        public void ProcessReview_IncorrectAnswer_ResetsRepetitionCount()
        {
            // Arrange
            int currentInterval = 30;
            int currentRepetitionCount = 5;
            double currentEFactor = 2.2;
            int quality = 2; // Below minimum passing quality

            // Act
            var (newInterval, newRepetitionCount, newEFactor) =
                ChineseVocab.SRS.SRSEngine.ProcessReview(currentInterval, currentRepetitionCount, currentEFactor, quality);

            // Assert
            Assert.Equal(0, newRepetitionCount);
            Assert.Equal(1, newInterval);
            Assert.True(newEFactor < currentEFactor);
        }

        [Fact]
        public void IsValidQualityScore_ValidScores_ReturnsTrue()
        {
            // Assert
            Assert.True(ChineseVocab.SRS.SRSEngine.IsValidQualityScore(0));
            Assert.True(ChineseVocab.SRS.SRSEngine.IsValidQualityScore(3));
            Assert.True(ChineseVocab.SRS.SRSEngine.IsValidQualityScore(5));
        }

        [Fact]
        public void IsValidQualityScore_InvalidScores_ReturnsFalse()
        {
            // Assert
            Assert.False(ChineseVocab.SRS.SRSEngine.IsValidQualityScore(-1));
            Assert.False(ChineseVocab.SRS.SRSEngine.IsValidQualityScore(6));
            Assert.False(ChineseVocab.SRS.SRSEngine.IsValidQualityScore(10));
        }

        [Fact]
        public void QualityScoreToDescription_ValidScore_ReturnsDescription()
        {
            // Assert
            Assert.Equal("Полный провал (совсем не помню)",
                ChineseVocab.SRS.SRSEngine.QualityScoreToDescription(0));
            Assert.Equal("Правильно с затруднениями (вспомнил после размышлений)",
                ChineseVocab.SRS.SRSEngine.QualityScoreToDescription(3));
            Assert.Equal("Правильно мгновенно (знаю на отлично)",
                ChineseVocab.SRS.SRSEngine.QualityScoreToDescription(5));
        }

        [Fact]
        public void QualityScoreToDescription_InvalidScore_ReturnsUnknown()
        {
            // Assert
            Assert.Equal("Неизвестная оценка",
                ChineseVocab.SRS.SRSEngine.QualityScoreToDescription(-1));
            Assert.Equal("Неизвестная оценка",
                ChineseVocab.SRS.SRSEngine.QualityScoreToDescription(10));
        }

        [Fact]
        public void IsCardLearned_IntervalAboveThreshold_ReturnsTrue()
        {
            // Assert
            Assert.True(ChineseVocab.SRS.SRSEngine.IsCardLearned(30));
            Assert.True(ChineseVocab.SRS.SRSEngine.IsCardLearned(21));
            Assert.False(ChineseVocab.SRS.SRSEngine.IsCardLearned(20));
            Assert.False(ChineseVocab.SRS.SRSEngine.IsCardLearned(1));
        }

        [Fact]
        public void CalculateNextReviewDate_ReturnsCorrectDate()
        {
            // Arrange
            int currentInterval = 6;
            int repetitionCount = 2;
            double eFactor = 2.5;
            int quality = 4;
            DateTime referenceDate = new DateTime(2024, 1, 1);

            // Act
            DateTime result = ChineseVocab.SRS.SRSEngine.CalculateNextReviewDate(
                currentInterval, repetitionCount, eFactor, quality, referenceDate);

            // Assert
            DateTime expectedDate = referenceDate.AddDays(15); // 6 * 2.5 = 15
            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void CalculateForgettingFactor_RecentReview_ReturnsLowValue()
        {
            // Arrange
            double eFactor = 2.5;
            TimeSpan timeSinceLastReview = TimeSpan.FromDays(1);

            // Act
            double result = ChineseVocab.SRS.SRSEngine.CalculateForgettingFactor(eFactor, timeSinceLastReview);

            // Assert
            Assert.InRange(result, 0, 1);
            Assert.True(result < 0.5, "Forgetting factor should be low for recent review");
        }

        [Fact]
        public void CalculateForgettingFactor_LongTimeSinceReview_ReturnsHighValue()
        {
            // Arrange
            double eFactor = 1.5;
            TimeSpan timeSinceLastReview = TimeSpan.FromDays(30);

            // Act
            double result = ChineseVocab.SRS.SRSEngine.CalculateForgettingFactor(eFactor, timeSinceLastReview);

            // Assert
            Assert.InRange(result, 0, 1);
            Assert.True(result > 0.5, "Forgetting factor should be high for long time since review");
        }

        [Fact]
        public void AdjustIntervalForLongBreak_ShortBreak_ReturnsSameInterval()
        {
            // Arrange
            int currentInterval = 30;
            TimeSpan breakDuration = TimeSpan.FromDays(15); // Less than 2 * interval

            // Act
            int result = ChineseVocab.SRS.SRSEngine.AdjustIntervalForLongBreak(currentInterval, breakDuration);

            // Assert
            Assert.Equal(currentInterval, result);
        }

        [Fact]
        public void AdjustIntervalForLongBreak_LongBreak_ReturnsReducedInterval()
        {
            // Arrange
            int currentInterval = 30;
            TimeSpan breakDuration = TimeSpan.FromDays(100); // Much longer than 2 * interval

            // Act
            int result = ChineseVocab.SRS.SRSEngine.AdjustIntervalForLongBreak(currentInterval, breakDuration);

            // Assert
            Assert.True(result < currentInterval, "Interval should be reduced for long break");
            Assert.True(result >= 1, "Interval should be at least 1 day");
        }

        [Fact]
        public void CalculateConfidenceScore_NoReviews_ReturnsZero()
        {
            // Act
            double result = ChineseVocab.SRS.SRSEngine.CalculateConfidenceScore(0, 0, 0, 2.5);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateConfidenceScore_HighAccuracy_ReturnsHighScore()
        {
            // Arrange
            int correctStreak = 10;
            int totalCorrect = 20;
            int totalIncorrect = 5;
            double eFactor = 2.5;

            // Act
            double result = ChineseVocab.SRS.SRSEngine.CalculateConfidenceScore(
                correctStreak, totalCorrect, totalIncorrect, eFactor);

            // Assert
            Assert.InRange(result, 0, 1);
            Assert.True(result > 0.5, "Confidence score should be high for good performance");
        }

        [Fact]
        public void CalculateReviewPriority_OverdueCard_HasHigherPriority()
        {
            // Arrange
            DateTime nextReviewDate = DateTime.UtcNow.AddDays(-5); // Overdue by 5 days
            double eFactor = 1.5;
            int correctStreak = 3;
            int daysOverdue = 5;

            // Act
            double priority = ChineseVocab.SRS.SRSEngine.CalculateReviewPriority(
                nextReviewDate, eFactor, correctStreak, daysOverdue);

            // Arrange for comparison - not overdue card
            DateTime notOverdueDate = DateTime.UtcNow.AddDays(5);
            double notOverduePriority = ChineseVocab.SRS.SRSEngine.CalculateReviewPriority(
                notOverdueDate, eFactor, correctStreak, 0);

            // Assert
            Assert.True(priority > notOverduePriority, "Overdue cards should have higher priority");
        }

        [Fact]
        public void CreateInitialStats_ReturnsDefaultValues()
        {
            // Act
            var (interval, repetitionCount, eFactor) = ChineseVocab.SRS.SRSEngine.CreateInitialStats();

            // Assert
            Assert.Equal(ChineseVocab.SRS.SRSEngine.FirstReviewInterval, interval);
            Assert.Equal(0, repetitionCount);
            Assert.Equal(ChineseVocab.SRS.SRSEngine.DefaultEFactor, eFactor);
        }
    }
}
