using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChineseVocab.Models;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Интерфейс для работы с базой данных приложения.
    /// Предоставляет методы для работы с карточками, статистикой, колодами и другими сущностями.
    /// </summary>
    public interface IDatabaseService
    {
        #region Инициализация и управление базой данных

        /// <summary>
        /// Инициализирует базу данных (создает таблицы, если они не существуют).
        /// </summary>
        Task InitializeDatabaseAsync();

        /// <summary>
        /// Закрывает соединение с базой данных.
        /// </summary>
        Task CloseDatabaseAsync();

        /// <summary>
        /// Удаляет базу данных (для тестирования или сброса данных).
        /// </summary>
        Task DeleteDatabaseAsync();

        /// <summary>
        /// Проверяет, существует ли база данных.
        /// </summary>
        Task<bool> DatabaseExistsAsync();

        /// <summary>
        /// Выполняет резервное копирование базы данных.
        /// </summary>
        /// <param name="backupPath">Путь для сохранения резервной копии.</param>
        Task BackupDatabaseAsync(string backupPath);

        /// <summary>
        /// Восстанавливает базу данных из резервной копии.
        /// </summary>
        /// <param name="backupPath">Путь к резервной копии.</param>
        Task RestoreDatabaseAsync(string backupPath);

        #endregion

        #region Операции с карточками (Cards)

        /// <summary>
        /// Получает карточку по идентификатору.
        /// </summary>
        Task<Card> GetCardByIdAsync(int id);

        /// <summary>
        /// Получает все активные карточки.
        /// </summary>
        Task<List<Card>> GetAllCardsAsync();

        /// <summary>
        /// Получает карточки по уровню HSK.
        /// </summary>
        Task<List<Card>> GetCardsByHskLevelAsync(int hskLevel);

        /// <summary>
        /// Получает карточки по радикалу.
        /// </summary>
        Task<List<Card>> GetCardsByRadicalAsync(string radical);

        /// <summary>
        /// Получает карточки по типу иероглифа.
        /// </summary>
        Task<List<Card>> GetCardsByCharacterTypeAsync(string characterType);

        /// <summary>
        /// Получает карточки, которые нужно повторить (следующая дата повторения наступила).
        /// </summary>
        Task<List<Card>> GetCardsForReviewAsync();

        /// <summary>
        /// Получает карточки из определенной колоды.
        /// </summary>
        Task<List<Card>> GetCardsByDeckIdAsync(int deckId);

        /// <summary>
        /// Получает карточки по тегам.
        /// </summary>
        Task<List<Card>> GetCardsByTagsAsync(string tags);

        /// <summary>
        /// Ищет карточки по тексту (иероглифу, пиньиню или переводу).
        /// </summary>
        Task<List<Card>> SearchCardsAsync(string searchText);

        /// <summary>
        /// Создает новую карточку.
        /// </summary>
        Task<int> CreateCardAsync(Card card);

        /// <summary>
        /// Обновляет существующую карточку.
        /// </summary>
        Task<int> UpdateCardAsync(Card card);

        /// <summary>
        /// Удаляет карточку (помечает как неактивную).
        /// </summary>
        Task<int> SoftDeleteCardAsync(int id);

        /// <summary>
        /// Полностью удаляет карточку из базы данных.
        /// </summary>
        Task<int> HardDeleteCardAsync(int id);

        /// <summary>
        /// Получает количество карточек.
        /// </summary>
        Task<int> GetCardCountAsync();

        /// <summary>
        /// Получает количество карточек по уровню HSK.
        /// </summary>
        Task<int> GetCardCountByHskLevelAsync(int hskLevel);

        /// <summary>
        /// Импортирует карточки из внешнего источника.
        /// </summary>
        Task<int> ImportCardsAsync(List<Card> cards);

        #endregion

        #region Операции со статистикой SRS (SRSStatistics)

        /// <summary>
        /// Получает статистику SRS для карточки.
        /// </summary>
        Task<SRSStat> GetSRSStatisticsByCardIdAsync(int cardId);

        /// <summary>
        /// Получает всю статистику SRS.
        /// </summary>
        Task<List<SRSStat>> GetAllSRSStatisticsAsync();

        /// <summary>
        /// Создает или обновляет статистику SRS для карточки.
        /// </summary>
        Task<int> SaveSRSStatisticsAsync(SRSStat statistics);

        /// <summary>
        /// Удаляет статистику SRS для карточки.
        /// </summary>
        Task<int> DeleteSRSStatisticsAsync(int cardId);

        /// <summary>
        /// Получает статистику SRS по дате следующего повторения.
        /// </summary>
        Task<List<SRSStat>> GetSRSStatisticsByNextReviewDateAsync(DateTime date);

        /// <summary>
        /// Получает количество карточек для повторения на определенную дату.
        /// </summary>
        Task<int> GetReviewCountForDateAsync(DateTime date);

        /// <summary>
        /// Получает общую статистику изучения.
        /// </summary>
        Task<StudySummary> GetStudySummaryAsync();

        #endregion

        #region Операции с колодами (Decks)

        /// <summary>
        /// Получает все колоды.
        /// </summary>
        Task<List<Deck>> GetAllDecksAsync();

        /// <summary>
        /// Получает колоду по идентификатору.
        /// </summary>
        Task<Deck> GetDeckByIdAsync(int id);

        /// <summary>
        /// Создает новую колоду.
        /// </summary>
        Task<int> CreateDeckAsync(Deck deck);

        /// <summary>
        /// Обновляет существующую колоду.
        /// </summary>
        Task<int> UpdateDeckAsync(Deck deck);

        /// <summary>
        /// Удаляет колоду.
        /// </summary>
        Task<int> DeleteDeckAsync(int id);

        /// <summary>
        /// Добавляет карточку в колоду.
        /// </summary>
        Task<int> AddCardToDeckAsync(int cardId, int deckId);

        /// <summary>
        /// Удаляет карточку из колоды.
        /// </summary>
        Task<int> RemoveCardFromDeckAsync(int cardId, int deckId);

        /// <summary>
        /// Получает количество карточек в колоде.
        /// </summary>
        Task<int> GetCardCountInDeckAsync(int deckId);

        /// <summary>
        /// Получает карточки из колоды для повторения.
        /// </summary>
        Task<List<Card>> GetDeckCardsForReviewAsync(int deckId);

        #endregion

        #region Операции с примерами предложений (Sentences)

        /// <summary>
        /// Получает примеры предложений для карточки.
        /// </summary>
        Task<List<Sentence>> GetSentencesByCardIdAsync(int cardId);

        /// <summary>
        /// Получает все примеры предложений.
        /// </summary>
        Task<List<Sentence>> GetAllSentencesAsync();

        /// <summary>
        /// Создает новый пример предложения.
        /// </summary>
        Task<int> CreateSentenceAsync(Sentence sentence);

        /// <summary>
        /// Обновляет существующий пример предложения.
        /// </summary>
        Task<int> UpdateSentenceAsync(Sentence sentence);

        /// <summary>
        /// Удаляет пример предложения.
        /// </summary>
        Task<int> DeleteSentenceAsync(int id);

        /// <summary>
        /// Ищет примеры предложений по тексту.
        /// </summary>
        Task<List<Sentence>> SearchSentencesAsync(string searchText);

        #endregion

        #region Операции с типами иероглифов (CharacterTypes)

        /// <summary>
        /// Получает все типы иероглифов.
        /// </summary>
        Task<List<CharacterType>> GetAllCharacterTypesAsync();

        /// <summary>
        /// Получает тип иероглифа по идентификатору.
        /// </summary>
        Task<CharacterType> GetCharacterTypeByIdAsync(int id);

        /// <summary>
        /// Получает тип иероглифа по названию.
        /// </summary>
        Task<CharacterType> GetCharacterTypeByNameAsync(string name);

        /// <summary>
        /// Создает новый тип иероглифа.
        /// </summary>
        Task<int> CreateCharacterTypeAsync(CharacterType characterType);

        /// <summary>
        /// Обновляет существующий тип иероглифа.
        /// </summary>
        Task<int> UpdateCharacterTypeAsync(CharacterType characterType);

        /// <summary>
        /// Удаляет тип иероглифа.
        /// </summary>
        Task<int> DeleteCharacterTypeAsync(int id);

        #endregion

        #region Статистика и аналитика

        /// <summary>
        /// Получает статистику изучения за определенный период.
        /// </summary>
        Task<List<DailyStudyStats>> GetDailyStudyStatsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получает статистику успеваемости по уровням HSK.
        /// </summary>
        Task<List<HskLevelStats>> GetHskLevelStatsAsync();

        /// <summary>
        /// Получает статистику по типам иероглифов.
        /// </summary>
        Task<List<CharacterTypeStats>> GetCharacterTypeStatsAsync();

        /// <summary>
        /// Получает текущую серию правильных ответов.
        /// </summary>
        Task<int> GetCurrentStreakAsync();

        /// <summary>
        /// Получает максимальную серию правильных ответов.
        /// </summary>
        Task<int> GetMaxStreakAsync();

        /// <summary>
        /// Получает общее время, потраченное на изучение.
        /// </summary>
        Task<TimeSpan> GetTotalStudyTimeAsync();

        /// <summary>
        /// Получает среднюю оценку за карточки.
        /// </summary>
        Task<double> GetAverageRatingAsync();

        #endregion

        #region Транзакции и массовые операции

        /// <summary>
        /// Выполняет операцию в транзакции.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> action);

        /// <summary>
        /// Очищает все данные (только для тестирования).
        /// </summary>
        Task ClearAllDataAsync();

        /// <summary>
        /// Выполняет массовое обновление карточек.
        /// </summary>
        Task<int> BulkUpdateCardsAsync(List<Card> cards);

        /// <summary>
        /// Выполняет массовое обновление статистики SRS.
        /// </summary>
        Task<int> BulkUpdateSRSStatisticsAsync(List<SRSStat> statistics);

        #endregion
    }

    /// <summary>
    /// Краткая сводка по изучению.
    /// </summary>
    public class StudySummary
    {
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public int CardsToReview { get; set; }
        public int CardsDueToday { get; set; }
        public int CardsDueTomorrow { get; set; }
        public double AverageEaseFactor { get; set; }
        public int TotalStudyDays { get; set; }
        public DateTime LastStudyDate { get; set; }
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }
    }

    /// <summary>
    /// Статистика изучения по дням.
    /// </summary>
    public class DailyStudyStats
    {
        public DateTime Date { get; set; }
        public int CardsStudied { get; set; }
        public int CardsReviewed { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public double AccuracyRate => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
        public TimeSpan StudyTime { get; set; }
    }

    /// <summary>
    /// Статистика по уровням HSK.
    /// </summary>
    public class HskLevelStats
    {
        public int HskLevel { get; set; }
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public double LearningProgress => TotalCards > 0 ? (double)CardsLearned / TotalCards * 100 : 0;
        public double AverageRating { get; set; }
        public int CardsToReview { get; set; }
    }

    /// <summary>
    /// Статистика по типам иероглифов.
    /// </summary>
    public class CharacterTypeStats
    {
        public string CharacterType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalCards { get; set; }
        public int CardsLearned { get; set; }
        public double LearningProgress => TotalCards > 0 ? (double)CardsLearned / TotalCards * 100 : 0;
        public double AverageDifficulty { get; set; }
    }
}
