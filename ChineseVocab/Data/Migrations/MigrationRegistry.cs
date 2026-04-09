using System;
using System.Collections.Generic;

namespace ChineseVocab.Data.Migrations
{
    /// <summary>
    /// Реестр всех миграций базы данных.
    /// Предоставляет централизованный список всех миграций, отсортированных по идентификатору.
    /// </summary>
    public static class MigrationRegistry
    {
        private static readonly List<IMigration> _migrations = new List<IMigration>();

        /// <summary>
        /// Статический конструктор для инициализации реестра миграций.
        /// </summary>
        static MigrationRegistry()
        {
            InitializeMigrations();
        }

        /// <summary>
        /// Инициализирует список миграций.
        /// Все новые миграции должны быть добавлены здесь.
        /// </summary>
        private static void InitializeMigrations()
        {
            // Добавляем миграции в порядке их применения
            // Важно: миграции должны быть отсортированы по возрастанию Id

            // Начальная миграция - создает все таблицы
            AddMigration(new InitialMigration());

            // В будущем здесь будут добавляться новые миграции, например:
            // AddMigration(new AddAudioSupportMigration());
            // AddMigration(new AddProgressTrackingMigration());
            // AddMigration(new AddSocialFeaturesMigration());
        }

        /// <summary>
        /// Добавляет миграцию в реестр.
        /// </summary>
        /// <param name="migration">Миграция для добавления.</param>
        /// <exception cref="ArgumentNullException">Если migration равен null.</exception>
        /// <exception cref="ArgumentException">Если миграция с таким Id уже существует.</exception>
        public static void AddMigration(IMigration migration)
        {
            if (migration == null)
                throw new ArgumentNullException(nameof(migration));

            // Проверяем, нет ли уже миграции с таким Id
            if (_migrations.Exists(m => m.Id == migration.Id))
                throw new ArgumentException($"Миграция с Id {migration.Id} уже существует в реестре.");

            _migrations.Add(migration);

            // Сортируем миграции по Id после каждого добавления
            _migrations.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        /// <summary>
        /// Получает список всех миграций, отсортированных по идентификатору (от старых к новым).
        /// </summary>
        /// <returns>Список всех миграций.</returns>
        public static IReadOnlyList<IMigration> GetAllMigrations()
        {
            return _migrations.AsReadOnly();
        }

        /// <summary>
        /// Получает миграцию по идентификатору.
        /// </summary>
        /// <param name="migrationId">Идентификатор миграции.</param>
        /// <returns>Миграция с указанным идентификатором или null, если не найдена.</returns>
        public static IMigration GetMigrationById(long migrationId)
        {
            return _migrations.Find(m => m.Id == migrationId);
        }

        /// <summary>
        /// Получает последнюю миграцию (с наибольшим идентификатором).
        /// </summary>
        /// <returns>Последняя миграция или null, если реестр пуст.</returns>
        public static IMigration GetLatestMigration()
        {
            if (_migrations.Count == 0)
                return null;

            return _migrations[_migrations.Count - 1];
        }

        /// <summary>
        /// Получает идентификатор последней миграции.
        /// </summary>
        /// <returns>Идентификатор последней миграции или 0, если реестр пуст.</returns>
        public static long GetLatestMigrationId()
        {
            var latest = GetLatestMigration();
            return latest?.Id ?? 0;
        }

        /// <summary>
        /// Проверяет, существует ли миграция с указанным идентификатором.
        /// </summary>
        /// <param name="migrationId">Идентификатор миграции для проверки.</param>
        /// <returns>True, если миграция существует; иначе false.</returns>
        public static bool MigrationExists(long migrationId)
        {
            return _migrations.Exists(m => m.Id == migrationId);
        }

        /// <summary>
        /// Получает количество миграций в реестре.
        /// </summary>
        /// <returns>Количество миграций.</returns>
        public static int GetMigrationCount()
        {
            return _migrations.Count;
        }

        /// <summary>
        /// Получает описание реестра миграций.
        /// </summary>
        /// <returns>Строка с описанием всех миграций.</returns>
        public static string GetRegistryDescription()
        {
            var description = $"Реестр миграций: {_migrations.Count} миграций\n";

            foreach (var migration in _migrations)
            {
                description += $"  • {migration.FormatMigrationId()} - {migration.Description}\n";
            }

            return description;
        }

        /// <summary>
        /// Очищает реестр миграций (в основном для тестирования).
        /// </summary>
        public static void Clear()
        {
            _migrations.Clear();
        }

        /// <summary>
        /// Переинициализирует реестр миграций (в основном для тестирования).
        /// </summary>
        public static void Reinitialize()
        {
            Clear();
            InitializeMigrations();
        }
    }
}
