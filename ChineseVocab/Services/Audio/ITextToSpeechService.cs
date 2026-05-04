namespace ChineseVocab.Services.Audio;

/// <summary>
/// Сервис озвучки китайского текста, иероглифов и пиньиня
/// с использованием встроенного TextToSpeech платформы.
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>
    /// Озвучивает китайский иероглиф или слово.
    /// Автоматически выбирает локаль zh-CN.
    /// </summary>
    /// <param name="chineseText">Китайский текст (иероглиф, слово или фраза).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SpeakChineseAsync(string chineseText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Озвучивает пиньинь. Диакритические знаки предварительно очищаются,
    /// так как большинство TTS-движков не обрабатывают их корректно.
    /// </summary>
    /// <param name="pinyin">Пиньинь с диакритикой или цифровыми тонами.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SpeakPinyinAsync(string pinyin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Озвучивает предложение на китайском языке.
    /// Использует пониженную скорость для лучшего восприятия.
    /// </summary>
    /// <param name="chineseSentence">Китайское предложение.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SpeakSentenceAsync(string chineseSentence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Немедленно останавливает текущее воспроизведение.
    /// </summary>
    void Stop();

    /// <summary>
    /// Проверяет, поддерживается ли указанный язык на текущем устройстве.
    /// </summary>
    /// <param name="locale">Локаль (например, "zh-CN").</param>
    /// <returns>true, если язык поддерживается.</returns>
    Task<bool> IsLanguageSupportedAsync(string locale = "zh-CN");

    /// <summary>
    /// Устанавливает скорость речи.
    /// </summary>
    /// <param name="rate">Скорость в диапазоне 0.5 (медленно) – 2.0 (быстро). Значение по умолчанию: 1.0.</param>
    void SetSpeechRate(float rate);

    /// <summary>
    /// Устанавливает высоту тона речи.
    /// </summary>
    /// <param name="pitch">Тон в диапазоне 0.5 (низкий) – 2.0 (высокий). Значение по умолчанию: 1.0.</param>
    void SetPitch(float pitch);

    /// <summary>
    /// Возвращает текущую скорость речи.
    /// </summary>
    float SpeechRate { get; }

    /// <summary>
    /// Возвращает текущую высоту тона.
    /// </summary>
    float Pitch { get; }

    /// <summary>
    /// Возвращает true, если в данный момент идёт воспроизведение.
    /// </summary>
    bool IsSpeaking { get; }

    /// <summary>
    /// Событие завершения воспроизведения.
    /// Параметр: true — завершено успешно, false — прервано или ошибка.
    /// </summary>
    event EventHandler<bool>? SpeechCompleted;

    /// <summary>
    /// Возвращает список поддерживаемых локалей на устройстве.
    /// </summary>
    Task<IReadOnlyList<string>> GetSupportedLocalesAsync();
}
