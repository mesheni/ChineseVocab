using System;
using System.Threading.Tasks;
using SQLite;

namespace ChineseVocab.Data.Migrations
{
    /// <summary>
    /// Начальная миграция базы данных - создает все основные таблицы.
    /// </summary>
    public class InitialMigration : Migration
    {
        /// <summary>
        /// Инициализирует новую начальную миграцию.
        /// </summary>
        public InitialMigration()
            : base(20250101000000, "Initial database schema - creates all tables")
        {
        }

        /// <inheritdoc />
        public override async Task MigrateUpAsync(SQLiteAsyncConnection db)
        {
            Console.WriteLine("Applying InitialMigration: Creating database tables...");

            // Создаем таблицу Cards (Карточки)
            await db.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Cards (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Character TEXT NOT NULL,
                    Simplified TEXT NOT NULL,
                    Traditional TEXT NOT NULL,
                    Pinyin TEXT NOT NULL,
                    Definition TEXT NOT NULL,
                    HskLevel INTEGER NOT NULL DEFAULT 0,
                    StrokeCount INTEGER NOT NULL DEFAULT 0,
                    Radical TEXT NOT NULL DEFAULT '',
                    CharacterType TEXT NOT NULL DEFAULT '',
                    Components TEXT NOT NULL DEFAULT '',
                    StrokeOrder TEXT NOT NULL DEFAULT '',
                    CreatedDate DATETIME NOT NULL,
                    ModifiedDate DATETIME NOT NULL,
                    IsActive BOOLEAN NOT NULL DEFAULT 1,
                    Tags TEXT NOT NULL DEFAULT '',
                    FrequencyRank INTEGER NOT NULL DEFAULT 0,
                    Notes TEXT NOT NULL DEFAULT '',
                    ImageUrl TEXT NOT NULL DEFAULT '',
                    AudioUrl TEXT NOT NULL DEFAULT ''
                )");

            Console.WriteLine("  • Created table: Cards");

            // Создаем таблицу Decks (Колоды)
            await db.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Decks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT '',
                    Color TEXT NOT NULL DEFAULT '#512BD4',
                    Icon TEXT NOT NULL DEFAULT '📚',
                    HskLevel INTEGER NOT NULL DEFAULT 0,
                    IsActive BOOLEAN NOT NULL DEFAULT 1,
                    IsSystem BOOLEAN NOT NULL DEFAULT 0,
                    IsFavorite BOOLEAN NOT NULL DEFAULT 0,
                    CreatedDate DATETIME NOT NULL,
                    ModifiedDate DATETIME NOT NULL,
                    LastStudied DATETIME NOT NULL DEFAULT '1970-01-01 00:00:00',
                    CardCount INTEGER NOT NULL DEFAULT 0,
                    LearnedCardCount INTEGER NOT NULL DEFAULT 0,
                    Progress REAL NOT NULL DEFAULT 0.0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    Tags TEXT NOT NULL DEFAULT ''
                )");

            Console.WriteLine("  • Created table: Decks");

            // Создаем таблицу Sentences (Примеры предложений)
            await db.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Sentences (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CardId INTEGER NOT NULL,
                    ChineseText TEXT NOT NULL,
                    Pinyin TEXT NOT NULL,
                    Translation TEXT NOT NULL,
                    Explanation TEXT NOT NULL DEFAULT '',
                    DifficultyLevel INTEGER NOT NULL DEFAULT 1,
                    Source TEXT NOT NULL DEFAULT '',
                    Tags TEXT NOT NULL DEFAULT '',
                    IsActive BOOLEAN NOT NULL DEFAULT 1,
                    CreatedDate DATETIME NOT NULL,
                    ModifiedDate DATETIME NOT NULL,
                    ViewCount INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (CardId) REFERENCES Cards(Id) ON DELETE CASCADE
                )");

            Console.WriteLine("  • Created table: Sentences");

            // Создаем таблицу CharacterTypes (Типы иероглифов)
            await db.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS CharacterTypes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ChineseName TEXT NOT NULL,
                    Pinyin TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT '',
                    Category TEXT NOT NULL DEFAULT '',
                    Examples TEXT NOT NULL DEFAULT '',
                    FormationPrinciple TEXT NOT NULL DEFAULT '',
                    StrokePattern TEXT NOT NULL DEFAULT '',
                    Percentage REAL NOT NULL DEFAULT 0.0,
                    DifficultyLevel INTEGER NOT NULL DEFAULT 1,
                    Icon TEXT NOT NULL DEFAULT '🔤',
                    Color TEXT NOT NULL DEFAULT '#512BD4',
                    Source TEXT NOT NULL DEFAULT '',
                    IsActive BOOLEAN NOT NULL DEFAULT 1,
                    IsSystem BOOLEAN NOT NULL DEFAULT 0,
                    CreatedDate DATETIME NOT NULL,
                    ModifiedDate DATETIME NOT NULL
                )");

            Console.WriteLine("  • Created table: CharacterTypes");

            // Создаем таблицу SRSStatistics (Статистика SRS)
            await db.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS SRSStatistics (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CardId INTEGER NOT NULL,
                    NextReviewDate DATETIME NOT NULL,
                    Interval INTEGER NOT NULL DEFAULT 1,
                    EFactor REAL NOT NULL DEFAULT 2.5,
                    RepetitionCount INTEGER NOT NULL DEFAULT 0,
                    EaseScore INTEGER NOT NULL DEFAULT 3,
                    LastReviewed DATETIME NOT NULL DEFAULT '1970-01-01 00:00:00',
                    CorrectStreak INTEGER NOT NULL DEFAULT 0,
                    TotalCorrect INTEGER NOT NULL DEFAULT 0,
                    TotalIncorrect INTEGER NOT NULL DEFAULT 0,
                    TotalStudyTimeSeconds INTEGER NOT NULL DEFAULT 0,
                    IsLearned BOOLEAN NOT NULL DEFAULT 0,
                    LearnedDate DATETIME NOT NULL DEFAULT '1970-01-01 00:00:00',
                    ConfidenceLevel INTEGER NOT NULL DEFAULT 1,
                    Notes TEXT NOT NULL DEFAULT '',
                    CreatedDate DATETIME NOT NULL,
                    ModifiedDate DATETIME NOT NULL,
                    FOREIGN KEY (CardId) REFERENCES Cards(Id) ON DELETE CASCADE
                )");

            Console.WriteLine("  • Created table: SRSStatistics");

            // Создаем таблицу для связи многие-ко-многим между карточками и колодами
            await db.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS CardDeck (
                    CardId INTEGER NOT NULL,
                    DeckId INTEGER NOT NULL,
                    AddedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (CardId, DeckId),
                    FOREIGN KEY (CardId) REFERENCES Cards(Id) ON DELETE CASCADE,
                    FOREIGN KEY (DeckId) REFERENCES Decks(Id) ON DELETE CASCADE
                )");

            Console.WriteLine("  • Created table: CardDeck");

            // Создаем индексы для улучшения производительности
            await CreateIndexesAsync(db);

            Console.WriteLine("InitialMigration applied successfully!");
        }

        /// <inheritdoc />
        public override async Task MigrateDownAsync(SQLiteAsyncConnection db)
        {
            Console.WriteLine("Rolling back InitialMigration: Dropping all tables...");

            // Удаляем таблицы в обратном порядке (сначала зависимые)
            await db.ExecuteAsync("DROP TABLE IF EXISTS CardDeck");
            Console.WriteLine("  • Dropped table: CardDeck");

            await db.ExecuteAsync("DROP TABLE IF EXISTS SRSStatistics");
            Console.WriteLine("  • Dropped table: SRSStatistics");

            await db.ExecuteAsync("DROP TABLE IF EXISTS Sentences");
            Console.WriteLine("  • Dropped table: Sentences");

            await db.ExecuteAsync("DROP TABLE IF EXISTS CharacterTypes");
            Console.WriteLine("  • Dropped table: CharacterTypes");

            await db.ExecuteAsync("DROP TABLE IF EXISTS Decks");
            Console.WriteLine("  • Dropped table: Decks");

            await db.ExecuteAsync("DROP TABLE IF EXISTS Cards");
            Console.WriteLine("  • Dropped table: Cards");

            Console.WriteLine("InitialMigration rolled back successfully!");
        }

        /// <summary>
        /// Создает индексы для улучшения производительности запросов.
        /// </summary>
        private async Task CreateIndexesAsync(SQLiteAsyncConnection db)
        {
            // Индекс для поиска карточек по уровню HSK
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Cards_HskLevel ON Cards(HskLevel)");

            // Индекс для поиска карточек по радикалу
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Cards_Radical ON Cards(Radical)");

            // Индекс для поиска карточек по типу иероглифа
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Cards_CharacterType ON Cards(CharacterType)");

            // Индекс для поиска карточек по статусу активности
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Cards_IsActive ON Cards(IsActive)");

            // Индекс для поиска статистики SRS по дате следующего повторения
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_SRSStatistics_NextReviewDate ON SRSStatistics(NextReviewDate)");

            // Индекс для поиска статистики SRS по CardId
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_SRSStatistics_CardId ON SRSStatistics(CardId)");

            // Индекс для поиска предложений по CardId
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Sentences_CardId ON Sentences(CardId)");

            // Индекс для поиска типов иероглифов по категории
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_CharacterTypes_Category ON CharacterTypes(Category)");

            // Индекс для поиска типов иероглифов по активности
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_CharacterTypes_IsActive ON CharacterTypes(IsActive)");

            // Индекс для связи CardDeck
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_CardDeck_CardId ON CardDeck(CardId)");
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_CardDeck_DeckId ON CardDeck(DeckId)");

            Console.WriteLine("  • Created performance indexes");
        }
    }
}
