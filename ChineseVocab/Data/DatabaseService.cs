using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using ChineseVocab.Models;
using ChineseVocab.Services;
using ChineseVocab.Data.Migrations;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Реализация сервиса базы данных с использованием SQLite.
    /// Предоставляет доступ к данным приложения: карточкам, статистике, колодам и т.д.
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private bool _isInitialized = false;

        /// <summary>
        /// Безопасный доступ к соединению с базой данных.
        /// Выбрасывает исключение, если база данных не инициализирована.
        /// </summary>
        private SQLiteAsyncConnection Database =>
            _database ?? throw new InvalidOperationException("База данных не инициализирована. Вызовите InitializeDatabaseAsync().");

        /// <summary>
        /// Конструктор сервиса базы данных.
        /// </summary>
        public DatabaseService()
        {
            // Путь к базе данных будет установлен при инициализации
        }

        #region Инициализация и управление базой данных

        /// <summary>
        /// Инициализирует базу данных (создает таблицы, если они не существуют).
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // Определяем путь к базе данных
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "chinesevocab.db3");
                _database = new SQLiteAsyncConnection(databasePath);

                // Создаем сервис миграций и применяем все ожидающие миграции
                var migrations = MigrationRegistry.GetAllMigrations();
                var migrationService = new MigrationService(_database, migrations);

                // Инициализируем таблицу миграций и применяем миграции
                await migrationService.InitializeAsync();
                int appliedMigrations = await migrationService.MigrateAsync();

                Console.WriteLine($"Применено миграций: {appliedMigrations}");

                // Загружаем начальные данные (типы иероглифов, HSK1 карточки и т.д.)
                var dataSeeder = new DataSeeder(this);
                bool dataSeeded = await dataSeeder.SeedAsync();
                if (dataSeeded)
                {
                    Console.WriteLine("Начальные данные успешно загружены.");
                }
                else
                {
                    Console.WriteLine("Начальные данные уже были загружены ранее или загрузка не потребовалась.");
                }

                _isInitialized = true;
                Console.WriteLine("База данных инициализирована успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации базы данных: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Закрывает соединение с базой данных.
        /// </summary>
        public async Task CloseDatabaseAsync()
        {
            if (_database != null)
            {
                await Database.CloseAsync();
                _database = null;
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Удаляет базу данных (для тестирования или сброса данных).
        /// </summary>
        public async Task DeleteDatabaseAsync()
        {
            try
            {
                await CloseDatabaseAsync();
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "chinesevocab.db3");
                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
                Console.WriteLine("База данных удалена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления базы данных: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Проверяет, существует ли база данных.
        /// </summary>
        public async Task<bool> DatabaseExistsAsync()
        {
            try
            {
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "chinesevocab.db3");
                return File.Exists(databasePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Выполняет резервное копирование базы данных.
        /// </summary>
        public async Task BackupDatabaseAsync(string backupPath)
        {
            try
            {
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "chinesevocab.db3");
                if (File.Exists(databasePath))
                {
                    File.Copy(databasePath, backupPath, true);
                    Console.WriteLine($"Резервная копия создана: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания резервной копии: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Восстанавливает базу данных из резервной копии.
        /// </summary>
        public async Task RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException($"Резервная копия не найдена: {backupPath}");
                }

                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "chinesevocab.db3");

                // Закрываем текущее соединение
                await CloseDatabaseAsync();

                // Копируем резервную копию
                File.Copy(backupPath, databasePath, true);

                // Повторно инициализируем базу данных
                await InitializeDatabaseAsync();

                Console.WriteLine($"База данных восстановлена из: {backupPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка восстановления базы данных: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Операции с карточками (Cards)

        /// <summary>
        /// Получает карточку по идентификатору.
        /// </summary>
        public async Task<Card> GetCardByIdAsync(int id)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Получает все активные карточки.
        /// </summary>
        public async Task<List<Card>> GetAllCardsAsync()
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().Where(c => c.IsActive).ToListAsync();
        }

        /// <summary>
        /// Получает карточки по уровню HSK.
        /// </summary>
        public async Task<List<Card>> GetCardsByHskLevelAsync(int hskLevel)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().Where(c => c.HskLevel == hskLevel && c.IsActive).ToListAsync();
        }

        /// <summary>
        /// Получает карточки по радикалу.
        /// </summary>
        public async Task<List<Card>> GetCardsByRadicalAsync(string radical)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().Where(c => c.Radical == radical && c.IsActive).ToListAsync();
        }

        /// <summary>
        /// Получает карточки по типу иероглифа.
        /// </summary>
        public async Task<List<Card>> GetCardsByCharacterTypeAsync(string characterType)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().Where(c => c.CharacterType == characterType && c.IsActive).ToListAsync();
        }

        /// <summary>
        /// Получает карточки, которые нужно повторить (следующая дата повторения наступила).
        /// </summary>
        public async Task<List<Card>> GetCardsForReviewAsync()
        {
            await EnsureInitializedAsync();
            var today = DateTime.UtcNow.Date;
            return await Database.QueryAsync<Card>(
                @"SELECT c.* FROM Cards c
                  INNER JOIN SRSStatistics s ON c.Id = s.CardId
                  WHERE c.IsActive = 1
                    AND s.NextReviewDate <= ?
                  ORDER BY s.NextReviewDate ASC",
                today);
        }

        /// <summary>
        /// Получает карточки из определенной колоды.
        /// </summary>
        public async Task<List<Card>> GetCardsByDeckIdAsync(int deckId)
        {
            await EnsureInitializedAsync();
            return await Database.QueryAsync<Card>(
                @"SELECT c.* FROM Cards c
                  INNER JOIN CardDeck cd ON c.Id = cd.CardId
                  WHERE cd.DeckId = ?
                    AND c.IsActive = 1
                  ORDER BY cd.AddedDate DESC",
                deckId);
        }

        /// <summary>
        /// Получает карточки по тегам.
        /// </summary>
        public async Task<List<Card>> GetCardsByTagsAsync(string tags)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().Where(c => c.Tags.Contains(tags) && c.IsActive).ToListAsync();
        }

        /// <summary>
        /// Ищет карточки по тексту (иероглифу, пиньиню или переводу).
        /// </summary>
        public async Task<List<Card>> SearchCardsAsync(string searchText)
        {
            await EnsureInitializedAsync();
            searchText = searchText.ToLower();
            return await Database.Table<Card>()
                .Where(c => c.IsActive &&
                    (c.Character.ToLower().Contains(searchText) ||
                     c.Pinyin.ToLower().Contains(searchText) ||
                     c.Definition.ToLower().Contains(searchText)))
                .ToListAsync();
        }

        /// <summary>
        /// Создает новую карточку.
        /// </summary>
        public async Task<int> CreateCardAsync(Card card)
        {
            await EnsureInitializedAsync();
            card.CreatedDate = DateTime.UtcNow;
            card.ModifiedDate = DateTime.UtcNow;
            return await Database.InsertAsync(card);
        }

        /// <summary>
        /// Обновляет существующую карточку.
        /// </summary>
        public async Task<int> UpdateCardAsync(Card card)
        {
            await EnsureInitializedAsync();
            card.ModifiedDate = DateTime.UtcNow;
            return await Database.UpdateAsync(card);
        }

        /// <summary>
        /// Удаляет карточку (помечает как неактивную).
        /// </summary>
        public async Task<int> SoftDeleteCardAsync(int id)
        {
            await EnsureInitializedAsync();
            var card = await GetCardByIdAsync(id);
            if (card != null)
            {
                card.IsActive = false;
                card.ModifiedDate = DateTime.UtcNow;
                return await UpdateCardAsync(card);
            }
            return 0;
        }

        /// <summary>
        /// Полностью удаляет карточку из базы данных.
        /// </summary>
        public async Task<int> HardDeleteCardAsync(int id)
        {
            await EnsureInitializedAsync();
            return await Database.DeleteAsync<Card>(id);
        }

        /// <summary>
        /// Получает количество карточек.
        /// </summary>
        public async Task<int> GetCardCountAsync()
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().CountAsync();
        }

        /// <summary>
        /// Получает количество карточек по уровню HSK.
        /// </summary>
        public async Task<int> GetCardCountByHskLevelAsync(int hskLevel)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Card>().Where(c => c.HskLevel == hskLevel && c.IsActive).CountAsync();
        }

        /// <summary>
        /// Импортирует карточки из внешнего источника.
        /// </summary>
        public async Task<int> ImportCardsAsync(List<Card> cards)
        {
            await EnsureInitializedAsync();
            int count = 0;
            foreach (var card in cards)
            {
                card.CreatedDate = DateTime.UtcNow;
                card.ModifiedDate = DateTime.UtcNow;
                await Database.InsertAsync(card);
                count++;
            }
            return count;
        }

        #endregion

        #region Операции со статистикой SRS (SRSStatistics)

        /// <summary>
        /// Получает статистику SRS для карточки.
        /// </summary>
        public async Task<SRSStat> GetSRSStatisticsByCardIdAsync(int cardId)
        {
            await EnsureInitializedAsync();
            return await Database.Table<SRSStat>().Where(s => s.CardId == cardId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Получает всю статистику SRS.
        /// </summary>
        public async Task<List<SRSStat>> GetAllSRSStatisticsAsync()
        {
            await EnsureInitializedAsync();
            return await Database.Table<SRSStat>().ToListAsync();
        }

        /// <summary>
        /// Создает или обновляет статистику SRS для карточки.
        /// </summary>
        public async Task<int> SaveSRSStatisticsAsync(SRSStat statistics)
        {
            await EnsureInitializedAsync();
            statistics.ModifiedDate = DateTime.UtcNow;

            var existing = await GetSRSStatisticsByCardIdAsync(statistics.CardId);
            if (existing == null)
            {
                statistics.CreatedDate = DateTime.UtcNow;
                return await Database.InsertAsync(statistics);
            }
            else
            {
                statistics.Id = existing.Id;
                statistics.CreatedDate = existing.CreatedDate;
                return await Database.UpdateAsync(statistics);
            }
        }

        /// <summary>
        /// Удаляет статистику SRS для карточки.
        /// </summary>
        public async Task<int> DeleteSRSStatisticsAsync(int cardId)
        {
            await EnsureInitializedAsync();
            return await Database.Table<SRSStat>().DeleteAsync(s => s.CardId == cardId);
        }

        /// <summary>
        /// Получает статистику SRS по дате следующего повторения.
        /// </summary>
        public async Task<List<SRSStat>> GetSRSStatisticsByNextReviewDateAsync(DateTime date)
        {
            await EnsureInitializedAsync();
            return await Database.Table<SRSStat>()
                .Where(s => s.NextReviewDate.Date == date.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Получает количество карточек для повторения на определенную дату.
        /// </summary>
        public async Task<int> GetReviewCountForDateAsync(DateTime date)
        {
            await EnsureInitializedAsync();
            return await Database.Table<SRSStat>()
                .Where(s => s.NextReviewDate.Date == date.Date)
                .CountAsync();
        }

        /// <summary>
        /// Получает общую статистику изучения.
        /// </summary>
        public async Task<StudySummary> GetStudySummaryAsync()
        {
            await EnsureInitializedAsync();

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Общее количество активных карточек
            int totalCards = await Database.Table<Card>().Where(c => c.IsActive).CountAsync();

            // Количество выученных карточек (IsLearned = true)
            int cardsLearned = await Database.Table<SRSStat>().Where(s => s.IsLearned).CountAsync();

            // Количество карточек для повторения (дата наступила)
            int cardsToReview = await Database.Table<SRSStat>()
                .Where(s => s.NextReviewDate <= today)
                .CountAsync();

            // Количество карточек к повторению сегодня
            int cardsDueToday = await Database.Table<SRSStat>()
                .Where(s => s.NextReviewDate.Date == today)
                .CountAsync();

            // Количество карточек к повторению завтра
            int cardsDueTomorrow = await Database.Table<SRSStat>()
                .Where(s => s.NextReviewDate.Date == tomorrow)
                .CountAsync();

            // Средний E-Factor по всем карточкам
            var allStats = await Database.Table<SRSStat>().ToListAsync();
            double avgEFactor = allStats.Count > 0 ? allStats.Average(s => s.EFactor) : 2.5;

            // Количество дней с активностью (уникальные даты LastReviewed)
            int totalStudyDays = allStats
                .Where(s => s.LastReviewed > DateTime.MinValue)
                .Select(s => s.LastReviewed.Date)
                .Distinct()
                .Count();

            // Дата последнего изучения
            DateTime lastStudyDate = allStats.Count > 0
                ? allStats.Max(s => s.LastReviewed)
                : DateTime.MinValue;

            // Текущая серия (максимальный CorrectStreak среди всех карточек)
            int currentStreak = allStats.Count > 0 ? allStats.Max(s => s.CorrectStreak) : 0;

            // Максимальная серия — сохраняется в БД? Пока используем текущую
            int maxStreak = currentStreak;

            return new StudySummary
            {
                TotalCards = totalCards,
                CardsLearned = cardsLearned,
                CardsToReview = cardsToReview,
                CardsDueToday = cardsDueToday,
                CardsDueTomorrow = cardsDueTomorrow,
                AverageEaseFactor = Math.Round(avgEFactor, 2),
                TotalStudyDays = totalStudyDays,
                LastStudyDate = lastStudyDate,
                CurrentStreak = currentStreak,
                MaxStreak = maxStreak
            };
        }

        #endregion

        #region Операции с колодами (Decks)

        /// <summary>
        /// Получает все колоды.
        /// </summary>
        public async Task<List<Deck>> GetAllDecksAsync()
        {
            await EnsureInitializedAsync();
            return await Database.Table<Deck>().Where(d => d.IsActive).ToListAsync();
        }

        /// <summary>
        /// Получает колоду по идентификатору.
        /// </summary>
        public async Task<Deck> GetDeckByIdAsync(int id)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Deck>().Where(d => d.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Создает новую колоду.
        /// </summary>
        public async Task<int> CreateDeckAsync(Deck deck)
        {
            await EnsureInitializedAsync();
            deck.CreatedDate = DateTime.UtcNow;
            deck.ModifiedDate = DateTime.UtcNow;
            return await Database.InsertAsync(deck);
        }

        /// <summary>
        /// Обновляет существующую колоду.
        /// </summary>
        public async Task<int> UpdateDeckAsync(Deck deck)
        {
            await EnsureInitializedAsync();
            deck.ModifiedDate = DateTime.UtcNow;
            return await Database.UpdateAsync(deck);
        }

        /// <summary>
        /// Удаляет колоду.
        /// </summary>
        public async Task<int> DeleteDeckAsync(int id)
        {
            await EnsureInitializedAsync();
            return await Database.DeleteAsync<Deck>(id);
        }

        /// <summary>
        /// Добавляет карточку в колоду. Если связь уже существует, ничего не делает.
        /// </summary>
        public async Task<int> AddCardToDeckAsync(int cardId, int deckId)
        {
            await EnsureInitializedAsync();
            return await Database.ExecuteAsync(
                "INSERT OR IGNORE INTO CardDeck (CardId, DeckId, AddedDate) VALUES (?, ?, ?)",
                cardId, deckId, DateTime.UtcNow);
        }

        /// <summary>
        /// Удаляет карточку из колоды.
        /// </summary>
        public async Task<int> RemoveCardFromDeckAsync(int cardId, int deckId)
        {
            await EnsureInitializedAsync();
            return await Database.ExecuteAsync(
                "DELETE FROM CardDeck WHERE CardId = ? AND DeckId = ?",
                cardId, deckId);
        }

        /// <summary>
        /// Получает количество карточек в колоде.
        /// </summary>
        public async Task<int> GetCardCountInDeckAsync(int deckId)
        {
            await EnsureInitializedAsync();
            var result = await Database.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM CardDeck WHERE DeckId = ?",
                deckId);
            return result;
        }

        /// <summary>
        /// Получает карточки из колоды, которые нужно повторить (дата повторения наступила).
        /// </summary>
        public async Task<List<Card>> GetDeckCardsForReviewAsync(int deckId)
        {
            await EnsureInitializedAsync();
            var today = DateTime.UtcNow.Date;
            return await Database.QueryAsync<Card>(
                @"SELECT c.* FROM Cards c
                  INNER JOIN CardDeck cd ON c.Id = cd.CardId
                  INNER JOIN SRSStatistics s ON c.Id = s.CardId
                  WHERE cd.DeckId = ?
                    AND c.IsActive = 1
                    AND s.NextReviewDate <= ?
                  ORDER BY s.NextReviewDate ASC",
                deckId, today);
        }

        #endregion

        #region Операции с примерами предложений (Sentences)

        /// <summary>
        /// Получает примеры предложений для карточки.
        /// </summary>
        public async Task<List<Sentence>> GetSentencesByCardIdAsync(int cardId)
        {
            await EnsureInitializedAsync();
            return await Database.Table<Sentence>().Where(s => s.CardId == cardId && s.IsActive).ToListAsync();
        }

        /// <summary>
        /// Получает все примеры предложений.
        /// </summary>
        public async Task<List<Sentence>> GetAllSentencesAsync()
        {
            await EnsureInitializedAsync();
            return await Database.Table<Sentence>().Where(s => s.IsActive).ToListAsync();
        }

        /// <summary>
        /// Создает новый пример предложения.
        /// </summary>
        public async Task<int> CreateSentenceAsync(Sentence sentence)
        {
            await EnsureInitializedAsync();
            sentence.CreatedDate = DateTime.UtcNow;
            sentence.ModifiedDate = DateTime.UtcNow;
            return await Database.InsertAsync(sentence);
        }

        /// <summary>
        /// Обновляет существующий пример предложения.
        /// </summary>
        public async Task<int> UpdateSentenceAsync(Sentence sentence)
        {
            await EnsureInitializedAsync();
            sentence.ModifiedDate = DateTime.UtcNow;
            return await Database.UpdateAsync(sentence);
        }

        /// <summary>
        /// Удаляет пример предложения.
        /// </summary>
        public async Task<int> DeleteSentenceAsync(int id)
        {
            await EnsureInitializedAsync();
            return await Database.DeleteAsync<Sentence>(id);
        }

        /// <summary>
        /// Ищет примеры предложений по тексту.
        /// </summary>
        public async Task<List<Sentence>> SearchSentencesAsync(string searchText)
        {
            await EnsureInitializedAsync();
            searchText = searchText.ToLower();
            return await Database.Table<Sentence>()
                .Where(s => s.IsActive &&
                    (s.ChineseText.ToLower().Contains(searchText) ||
                     s.Pinyin.ToLower().Contains(searchText) ||
                     s.Translation.ToLower().Contains(searchText)))
                .ToListAsync();
        }

        #endregion

        #region Операции с типами иероглифов (CharacterTypes)

        /// <summary>
        /// Получает все типы иероглифов.
        /// </summary>
        public async Task<List<CharacterType>> GetAllCharacterTypesAsync()
        {
            await EnsureInitializedAsync();
            return await Database.Table<CharacterType>().Where(ct => ct.IsActive).ToListAsync();
        }

        /// <summary>
        /// Получает тип иероглифа по идентификатору.
        /// </summary>
        public async Task<CharacterType> GetCharacterTypeByIdAsync(int id)
        {
            await EnsureInitializedAsync();
            return await Database.Table<CharacterType>().Where(ct => ct.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Получает тип иероглифа по названию.
        /// </summary>
        public async Task<CharacterType> GetCharacterTypeByNameAsync(string name)
        {
            await EnsureInitializedAsync();
            return await Database.Table<CharacterType>().Where(ct => ct.Name == name && ct.IsActive).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Создает новый тип иероглифа.
        /// </summary>
        public async Task<int> CreateCharacterTypeAsync(CharacterType characterType)
        {
            await EnsureInitializedAsync();
            characterType.CreatedDate = DateTime.UtcNow;
            characterType.ModifiedDate = DateTime.UtcNow;
            return await Database.InsertAsync(characterType);
        }

        /// <summary>
        /// Обновляет существующий тип иероглифа.
        /// </summary>
        public async Task<int> UpdateCharacterTypeAsync(CharacterType characterType)
        {
            await EnsureInitializedAsync();
            characterType.ModifiedDate = DateTime.UtcNow;
            return await Database.UpdateAsync(characterType);
        }

        /// <summary>
        /// Удаляет тип иероглифа.
        /// </summary>
        public async Task<int> DeleteCharacterTypeAsync(int id)
        {
            await EnsureInitializedAsync();
            return await Database.DeleteAsync<CharacterType>(id);
        }

        #endregion

        #region Статистика и аналитика

        /// <summary>
        /// Получает статистику изучения за определенный период.
        /// </summary>
        public async Task<List<DailyStudyStats>> GetDailyStudyStatsAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();
            // Временная заглушка
            return new List<DailyStudyStats>();
        }

        /// <summary>
        /// Получает статистику успеваемости по уровням HSK.
        /// </summary>
        public async Task<List<HskLevelStats>> GetHskLevelStatsAsync()
        {
            await EnsureInitializedAsync();
            // Временная заглушка
            return new List<HskLevelStats>();
        }

        /// <summary>
        /// Получает статистику по типам иероглифов.
        /// </summary>
        public async Task<List<CharacterTypeStats>> GetCharacterTypeStatsAsync()
        {
            await EnsureInitializedAsync();
            // Временная заглушка
            return new List<CharacterTypeStats>();
        }

        /// <summary>
        /// Получает текущую серию правильных ответов.
        /// </summary>
        public async Task<int> GetCurrentStreakAsync()
        {
            await EnsureInitializedAsync();
            return 0;
        }

        /// <summary>
        /// Получает максимальную серию правильных ответов.
        /// </summary>
        public async Task<int> GetMaxStreakAsync()
        {
            await EnsureInitializedAsync();
            return 0;
        }

        /// <summary>
        /// Получает общее время, потраченное на изучение.
        /// </summary>
        public async Task<TimeSpan> GetTotalStudyTimeAsync()
        {
            await EnsureInitializedAsync();
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Получает среднюю оценку за карточки.
        /// </summary>
        public async Task<double> GetAverageRatingAsync()
        {
            await EnsureInitializedAsync();
            return 0.0;
        }

        #endregion

        #region Транзакции и массовые операции

        /// <summary>
        /// Выполняет операцию в транзакции.
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await EnsureInitializedAsync();
            await Database.RunInTransactionAsync(conn =>
            {
                // SQLite-net не поддерживает async в транзакциях, выполняем синхронно
                action().GetAwaiter().GetResult();
            });
        }

        /// <summary>
        /// Очищает все данные (только для тестирования).
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            await EnsureInitializedAsync();
            await Database.DeleteAllAsync<Card>();
            await Database.DeleteAllAsync<Deck>();
            await Database.DeleteAllAsync<Sentence>();
            await Database.DeleteAllAsync<CharacterType>();
            await Database.DeleteAllAsync<SRSStat>();
        }

        /// <summary>
        /// Выполняет массовое обновление карточек.
        /// </summary>
        public async Task<int> BulkUpdateCardsAsync(List<Card> cards)
        {
            await EnsureInitializedAsync();
            int count = 0;
            foreach (var card in cards)
            {
                card.ModifiedDate = DateTime.UtcNow;
                await Database.UpdateAsync(card);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Выполняет массовое обновление статистики SRS.
        /// </summary>
        public async Task<int> BulkUpdateSRSStatisticsAsync(List<SRSStat> statistics)
        {
            await EnsureInitializedAsync();
            // Временная заглушка
            return 0;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет, инициализирована ли база данных, и если нет - инициализирует.
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized || _database == null)
            {
                await InitializeDatabaseAsync();
            }
        }

        #endregion
    }
}
