using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace ChineseVocab.Data.Migrations
{
    /// <summary>
    /// Сервис для управления миграциями базы данных.
    /// Отвечает за применение и откат миграций, отслеживание состояния.
    /// </summary>
    public class MigrationService
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly List<IMigration> _migrations;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса миграций.
        /// </summary>
        /// <param name="database">Соединение с базой данных.</param>
        /// <param name="migrations">Список миграций для управления.</param>
        public MigrationService(SQLiteAsyncConnection database, IEnumerable<IMigration> migrations)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _migrations = migrations?.ToList() ?? throw new ArgumentNullException(nameof(migrations));

            // Сортируем миграции по возрастанию идентификатора
            _migrations.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        /// <summary>
        /// Инициализирует таблицу миграций, если она не существует.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // Создаем таблицу для отслеживания миграций
                await _database.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS __Migrations (
                        Id INTEGER PRIMARY KEY,
                        Description TEXT NOT NULL,
                        AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    )");

                Console.WriteLine("Таблица миграций инициализирована");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации таблицы миграций: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает список уже примененных миграций.
        /// </summary>
        /// <returns>Словарь, где ключ - идентификатор миграции, значение - описание.</returns>
        public async Task<Dictionary<long, string>> GetAppliedMigrationsAsync()
        {
            try
            {
                var appliedMigrations = await _database.QueryAsync<MigrationRecord>(
                    "SELECT Id, Description FROM __Migrations ORDER BY Id");

                return appliedMigrations.ToDictionary(m => m.Id, m => m.Description);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения примененных миграций: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает список миграций, которые еще не были применены.
        /// </summary>
        /// <returns>Список ожидающих миграций, отсортированных по возрастанию идентификатора.</returns>
        public async Task<List<IMigration>> GetPendingMigrationsAsync()
        {
            var appliedMigrations = await GetAppliedMigrationsAsync();
            return _migrations
                .Where(m => !appliedMigrations.ContainsKey(m.Id))
                .OrderBy(m => m.Id)
                .ToList();
        }

        /// <summary>
        /// Применяет все ожидающие миграции.
        /// </summary>
        /// <returns>Количество примененных миграций.</returns>
        public async Task<int> MigrateAsync()
        {
            try
            {
                await InitializeAsync();
                var pendingMigrations = await GetPendingMigrationsAsync();

                if (pendingMigrations.Count == 0)
                {
                    Console.WriteLine("Нет ожидающих миграций. База данных актуальна.");
                    return 0;
                }

                Console.WriteLine($"Найдено {pendingMigrations.Count} ожидающих миграций:");

                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"  • {migration.FormatMigrationId()} - {migration.Description}");
                }

                int appliedCount = 0;

                foreach (var migration in pendingMigrations)
                {
                    await ApplyMigrationAsync(migration);
                    appliedCount++;
                }

                Console.WriteLine($"Применено {appliedCount} миграций успешно.");
                return appliedCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка применения миграций: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Применяет конкретную миграцию (миграция вверх).
        /// </summary>
        /// <param name="migrationId">Идентификатор миграции.</param>
        /// <returns>True, если миграция была успешно применена; false, если миграция уже была применена или не найдена.</returns>
        public async Task<bool> MigrateUpAsync(long migrationId)
        {
            try
            {
                await InitializeAsync();

                var migration = _migrations.FirstOrDefault(m => m.Id == migrationId);
                if (migration == null)
                {
                    Console.WriteLine($"Миграция с ID {migrationId} не найдена.");
                    return false;
                }

                var appliedMigrations = await GetAppliedMigrationsAsync();
                if (appliedMigrations.ContainsKey(migrationId))
                {
                    Console.WriteLine($"Миграция с ID {migrationId} уже применена.");
                    return false;
                }

                await ApplyMigrationAsync(migration);
                Console.WriteLine($"Миграция с ID {migrationId} успешно применена.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка применения миграции с ID {migrationId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Откатывает конкретную миграцию (миграция вниз).
        /// </summary>
        /// <param name="migrationId">Идентификатор миграции.</param>
        /// <returns>True, если миграция была успешно откачена; false, если миграция не была применена или не найдена.</returns>
        public async Task<bool> MigrateDownAsync(long migrationId)
        {
            try
            {
                await InitializeAsync();

                var migration = _migrations.FirstOrDefault(m => m.Id == migrationId);
                if (migration == null)
                {
                    Console.WriteLine($"Миграция с ID {migrationId} не найдена.");
                    return false;
                }

                var appliedMigrations = await GetAppliedMigrationsAsync();
                if (!appliedMigrations.ContainsKey(migrationId))
                {
                    Console.WriteLine($"Миграция с ID {migrationId} не была применена.");
                    return false;
                }

                await RollbackMigrationAsync(migration);
                Console.WriteLine($"Миграция с ID {migrationId} успешно откачена.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отката миграции с ID {migrationId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Откатывает последние N миграций.
        /// </summary>
        /// <param name="count">Количество миграций для отката.</param>
        /// <returns>Количество успешно откаченных миграций.</returns>
        public async Task<int> RollbackAsync(int count)
        {
            try
            {
                await InitializeAsync();

                var appliedMigrations = await GetAppliedMigrationsAsync();
                var migrationsToRollback = _migrations
                    .Where(m => appliedMigrations.ContainsKey(m.Id))
                    .OrderByDescending(m => m.Id)
                    .Take(count)
                    .ToList();

                if (migrationsToRollback.Count == 0)
                {
                    Console.WriteLine("Нет примененных миграций для отката.");
                    return 0;
                }

                Console.WriteLine($"Откатывается {migrationsToRollback.Count} миграций:");

                int rolledBackCount = 0;
                foreach (var migration in migrationsToRollback)
                {
                    Console.WriteLine($"  • {migration.FormatMigrationId()} - {migration.Description}");
                    await RollbackMigrationAsync(migration);
                    rolledBackCount++;
                }

                Console.WriteLine($"Откачено {rolledBackCount} миграций успешно.");
                return rolledBackCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отката миграций: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает текущее состояние миграций.
        /// </summary>
        /// <returns>Строка с информацией о состоянии миграций.</returns>
        public async Task<string> GetStatusAsync()
        {
            try
            {
                await InitializeAsync();

                var appliedMigrations = await GetAppliedMigrationsAsync();
                var pendingMigrations = await GetPendingMigrationsAsync();

                var result = $"Состояние миграций:\n";
                result += $"Всего миграций в системе: {_migrations.Count}\n";
                result += $"Применено миграций: {appliedMigrations.Count}\n";
                result += $"Ожидает применения: {pendingMigrations.Count}\n";

                if (appliedMigrations.Count > 0)
                {
                    result += "\nПримененные миграции:\n";
                    foreach (var migration in _migrations.Where(m => appliedMigrations.ContainsKey(m.Id)))
                    {
                        result += $"  • {migration.FormatMigrationId()} - {migration.Description}\n";
                    }
                }

                if (pendingMigrations.Count > 0)
                {
                    result += "\nОжидающие миграции:\n";
                    foreach (var migration in pendingMigrations)
                    {
                        result += $"  • {migration.FormatMigrationId()} - {migration.Description}\n";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Ошибка получения состояния миграций: {ex.Message}";
            }
        }

        /// <summary>
        /// Применяет миграцию и регистрирует ее в таблице миграций.
        /// </summary>
        /// <param name="migration">Миграция для применения.</param>
        private async Task ApplyMigrationAsync(IMigration migration)
        {
            try
            {
                Console.WriteLine($"Применение миграции: {migration.FormatMigrationId()} - {migration.Description}");

                // Начинаем транзакцию
                await _database.RunInTransactionAsync(connection =>
                {
                    // Применяем миграцию
                    migration.MigrateUpAsync(_database).Wait();

                    // Регистрируем миграцию как примененную
                    connection.Execute(
                        "INSERT INTO __Migrations (Id, Description) VALUES (?, ?)",
                        migration.Id, migration.Description);

                    Console.WriteLine($"Миграция {migration.FormatMigrationId()} успешно применена.");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка применения миграции {migration.FormatMigrationId()}: {ex.Message}");
                throw new Exception($"Ошибка применения миграции {migration.FormatMigrationId()}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Откатывает миграцию и удаляет ее из таблицы миграций.
        /// </summary>
        /// <param name="migration">Миграция для отката.</param>
        private async Task RollbackMigrationAsync(IMigration migration)
        {
            try
            {
                Console.WriteLine($"Откат миграции: {migration.FormatMigrationId()} - {migration.Description}");

                // Начинаем транзакцию
                await _database.RunInTransactionAsync(connection =>
                {
                    // Откатываем миграцию
                    migration.MigrateDownAsync(_database).Wait();

                    // Удаляем запись о миграции
                    connection.Execute(
                        "DELETE FROM __Migrations WHERE Id = ?",
                        migration.Id);

                    Console.WriteLine($"Миграция {migration.FormatMigrationId()} успешно откачена.");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отката миграции {migration.FormatMigrationId()}: {ex.Message}");
                throw new Exception($"Ошибка отката миграции {migration.FormatMigrationId()}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Вспомогательный класс для чтения записей о миграциях из базы данных.
        /// </summary>
        private class MigrationRecord
        {
            public long Id { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}
