namespace ChineseVocab.Services
{
    /// <summary>
    /// Заглушка сервиса уведомлений.
    /// Логирует вызовы в консоль. Полноценная реализация
    /// с локальными push-уведомлениями будет добавлена позже.
    /// </summary>
    public class NotificationService : INotificationService
    {
        /// <inheritdoc />
        public Task SendNotificationAsync(string title, string message)
        {
            // Заглушка: логируем вместо реальной отправки
            Console.WriteLine($"[Notification] {title}: {message}");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> RequestNotificationPermissionAsync()
        {
            // Заглушка: разрешение всегда получено
            Console.WriteLine("[Notification] Permission requested (stub) — granted.");
            return Task.FromResult(true);
        }
    }
}
