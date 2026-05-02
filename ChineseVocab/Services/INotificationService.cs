namespace ChineseVocab.Services
{
    /// <summary>
    /// Интерфейс сервиса уведомлений.
    /// Отвечает за отправку локальных push-уведомлений пользователю
    /// о необходимости повторить карточки.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Отправляет локальное уведомление с указанным заголовком и сообщением.
        /// </summary>
        /// <param name="title">Заголовок уведомления.</param>
        /// <param name="message">Текст уведомления.</param>
        Task SendNotificationAsync(string title, string message);

        /// <summary>
        /// Запрашивает у пользователя разрешение на отправку уведомлений.
        /// </summary>
        /// <returns>True, если разрешение получено.</returns>
        Task<bool> RequestNotificationPermissionAsync();
    }
}
