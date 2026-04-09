using System;
using System.Threading.Tasks;
using SQLite;

namespace ChineseVocab.Data.Migrations
{
    /// <summary>
    /// Тип миграции: вверх (применить изменения) или вниз (откатить изменения).
    /// </summary>
    public enum MigrationDirection
    {
        Up,
        Down
    }

    /// <summary>
    /// Интерфейс для миграции базы данных.
    /// Каждая миграция должна реализовывать этот интерфейс.
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Уникальный идентификатор миграции.
        /// Формат: YYYYMMDDHHMMSS (год, месяц, день, час, минута, секунда).
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Описание миграции (что она делает).
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Применить миграцию (миграция вверх).
        /// </summary>
        /// <param name="db">Асинхронное соединение с базой данных.</param>
        Task MigrateUpAsync(SQLite.SQLiteAsyncConnection db);

        /// <summary>
        /// Откатить миграцию (миграция вниз).
        /// </summary>
        /// <param name="db">Асинхронное соединение с базой данных.</param>
        Task MigrateDownAsync(SQLite.SQLiteAsyncConnection db);

        /// <summary>
        /// Форматирует идентификатор миграции в читаемый вид.
        /// </summary>
        string FormatMigrationId();
    }

    /// <summary>
    /// Базовый абстрактный класс для миграций.
    /// Упрощает создание миграций, предоставляя общую функциональность.
    /// </summary>
    public abstract class Migration : IMigration
    {
        /// <summary>
        /// Инициализирует новую миграцию с указанным идентификатором и описанием.
        /// </summary>
        /// <param name="id">Уникальный идентификатор миграции (формат: YYYYMMDDHHMMSS).</param>
        /// <param name="description">Описание миграции.</param>
        protected Migration(long id, string description)
        {
            Id = id;
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        /// <inheritdoc />
        public long Id { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public abstract Task MigrateUpAsync(SQLite.SQLiteAsyncConnection db);

        /// <inheritdoc />
        public abstract Task MigrateDownAsync(SQLite.SQLiteAsyncConnection db);

        /// <summary>
        /// Генерирует идентификатор миграции на основе текущей даты и времени.
        /// Формат: YYYYMMDDHHMMSS (год, месяц, день, час, минута, секунда).
        /// </summary>
        public static long GenerateMigrationId()
        {
            var now = DateTime.UtcNow;
            return now.Year * 10000000000L +
                   now.Month * 100000000L +
                   now.Day * 1000000L +
                   now.Hour * 10000L +
                   now.Minute * 100L +
                   now.Second;
        }

        /// <summary>
        /// Форматирует идентификатор миграции в читаемый вид.
        /// </summary>
        public string FormatMigrationId()
        {
            long id = Id;
            int seconds = (int)(id % 100);
            id /= 100;
            int minutes = (int)(id % 100);
            id /= 100;
            int hours = (int)(id % 100);
            id /= 100;
            int day = (int)(id % 100);
            id /= 100;
            int month = (int)(id % 100);
            id /= 100;
            int year = (int)id;

            return $"{year:D4}-{month:D2}-{day:D2} {hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        /// <summary>
        /// Возвращает строковое представление миграции (для отладки).
        /// </summary>
        public override string ToString()
        {
            return $"{FormatMigrationId()} - {Description}";
        }
    }
}
