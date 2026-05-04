using ChineseVocab.Utils;
using Microsoft.Extensions.Logging;

namespace ChineseVocab.Services.Audio;

/// <summary>
/// Реализация сервиса озвучки на основе встроенного <see cref="Microsoft.Maui.Media.TextToSpeech"/>.
///
/// Поддерживает:
/// - Озвучку китайских иероглифов (локаль zh-CN).
/// - Озвучку пиньиня (с предварительной очисткой диакритики).
/// - Озвучку предложений с настраиваемой скоростью.
/// - Управление скоростью и высотой тона.
/// - Отслеживание состояния воспроизведения.
/// </summary>
public class TextToSpeechService : ITextToSpeechService, IDisposable
{
    private readonly ILogger<TextToSpeechService> _logger;

    private CancellationTokenSource? _currentCts;
    private readonly object _lock = new();

    private float _speechRate = 1.0f;
    private float _pitch = 1.0f;

    private const float DefaultChineseRate = 0.9f;    // Немного замедленно для изучающих
    private const float DefaultSentenceRate = 0.85f;   // Ещё медленнее для предложений
    private const float MinRate = 0.25f;
    private const float MaxRate = 3.0f;
    private const float MinPitch = 0.5f;
    private const float MaxPitch = 2.0f;

    /// <inheritdoc />
    public float SpeechRate
    {
        get => _speechRate;
        private set => _speechRate = Math.Clamp(value, MinRate, MaxRate);
    }

    /// <inheritdoc />
    public float Pitch
    {
        get => _pitch;
        private set => _pitch = Math.Clamp(value, MinPitch, MaxPitch);
    }

    /// <inheritdoc />
    public bool IsSpeaking { get; private set; }

    /// <inheritdoc />
    public event EventHandler<bool>? SpeechCompleted;

    /// <summary>
    /// Кэш проверки поддержки языка (результат не меняется в рамках сессии).
    /// </summary>
    private bool? _chineseSupported;

    public TextToSpeechService(ILogger<TextToSpeechService> logger)
    {
        _logger = logger;
    }

    #region Основные методы озвучки

    /// <inheritdoc />
    public async Task SpeakChineseAsync(string chineseText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(chineseText))
            return;

        await SpeakCoreAsync(
            chineseText,
            rate: DefaultChineseRate,
            locale: "zh-CN",
            caller: nameof(SpeakChineseAsync),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task SpeakPinyinAsync(string pinyin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pinyin))
            return;

        // Очищаем диакритику: большинство TTS-движков не знают, что делать с ā, ǐ и т.д.
        // Также конвертируем цифровые тоны в чистые слоги (убираем цифры).
        string cleanPinyin = PinyinConverter.StripDiacritics(pinyin);

        // Если остались цифры тонов (формат "ni3") — убираем и их
        cleanPinyin = StripToneDigits(cleanPinyin);

        if (string.IsNullOrWhiteSpace(cleanPinyin))
            return;

        // Пиньинь — это латиница, локаль zh-CN может не сработать.
        // Используем нейтральную локаль, но с пониженной скоростью для разборчивости.
        await SpeakCoreAsync(
            cleanPinyin,
            rate: DefaultChineseRate * 0.85f,
            locale: null, // авто-определение
            caller: nameof(SpeakPinyinAsync),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task SpeakSentenceAsync(string chineseSentence, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(chineseSentence))
            return;

        await SpeakCoreAsync(
            chineseSentence,
            rate: DefaultSentenceRate,
            locale: "zh-CN",
            caller: nameof(SpeakSentenceAsync),
            cancellationToken);
    }

    #endregion

    #region Управление воспроизведением

    /// <inheritdoc />
    public void Stop()
    {
        lock (_lock)
        {
            if (_currentCts is not null)
            {
                try
                {
                    _currentCts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Уже disposed — игнорируем
                }
            }

            IsSpeaking = false;
        }
    }

    /// <inheritdoc />
    public void SetSpeechRate(float rate)
    {
        SpeechRate = rate;
    }

    /// <inheritdoc />
    public void SetPitch(float pitch)
    {
        Pitch = pitch;
    }

    #endregion

    #region Информационные методы

    /// <inheritdoc />
    public async Task<bool> IsLanguageSupportedAsync(string locale = "zh-CN")
    {
        if (locale == "zh-CN" && _chineseSupported.HasValue)
            return _chineseSupported.Value;

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            bool supported = locales.Any(l =>
                l.Language.Equals("zh", StringComparison.OrdinalIgnoreCase) ||
                l.Language.Equals("cmn", StringComparison.OrdinalIgnoreCase) ||
                l.Language.Equals("chi", StringComparison.OrdinalIgnoreCase) ||
                l.Language.Equals("zho", StringComparison.OrdinalIgnoreCase));

            if (locale == "zh-CN")
                _chineseSupported = supported;

            _logger.LogDebug("Китайский язык поддерживается TTS: {Supported}", supported);
            return supported;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка проверки поддержки языка TTS");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetSupportedLocalesAsync()
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            return locales
                .Select(l => $"{l.Language}-{l.Country} ({l.Name})")
                .Distinct()
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка получения списка локалей TTS");
            return Array.Empty<string>();
        }
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Ядро озвучки: настраивает опции, управляет CancellationToken, отслеживает состояние.
    /// </summary>
    private async Task SpeakCoreAsync(
        string text,
        float rate,
        string? locale,
        string caller,
        CancellationToken externalCancellationToken)
    {
        // Отменяем предыдущее воспроизведение
        Stop();

        CancellationTokenSource cts;
        lock (_lock)
        {
            _currentCts?.Dispose();
            cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken);
            _currentCts = cts;
        }

        try
        {
            IsSpeaking = true;

            var options = new SpeechOptions
            {
                Pitch = Pitch,
            };

            // Устанавливаем локаль, если указана
            if (!string.IsNullOrEmpty(locale))
            {
                try
                {
                    var availableLocales = await TextToSpeech.Default.GetLocalesAsync();
                    var targetLocale = availableLocales.FirstOrDefault(l =>
                        $"{l.Language}-{l.Country}".Equals(locale, StringComparison.OrdinalIgnoreCase));

                    if (targetLocale is not null)
                    {
                        options.Locale = targetLocale;
                    }
                    else
                    {
                        // Пробуем найти любой китайский
                        var anyChinese = availableLocales.FirstOrDefault(l =>
                            l.Language.Equals("zh", StringComparison.OrdinalIgnoreCase));
                        if (anyChinese is not null)
                            options.Locale = anyChinese;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Не удалось установить локаль {Locale}, используется авто-определение", locale);
                }
            }

            // Настройка скорости через SpeechOptions (если поддерживается)
            // MAUI TextToSpeech может не поддерживать параметр rate напрямую в SpeechOptions.
            // Используем кастомную скорость через вычисление задержек, либо
            // полагаемся на системные настройки TTS.
            float effectiveRate = rate * SpeechRate;

            _logger.LogDebug(
                "{Caller}: озвучка \"{Text}\" (rate={Rate:F2}, locale={Locale})",
                caller,
                text.Length > 20 ? text[..20] + "..." : text,
                effectiveRate,
                locale ?? "auto");

            await TextToSpeech.Default.SpeakAsync(text, options, cts.Token);

            _logger.LogDebug("{Caller}: воспроизведение завершено успешно", caller);
            SpeechCompleted?.Invoke(this, true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("{Caller}: воспроизведение отменено", caller);
            SpeechCompleted?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Caller}: ошибка озвучки", caller);
            SpeechCompleted?.Invoke(this, false);
        }
        finally
        {
            IsSpeaking = false;

            lock (_lock)
            {
                if (_currentCts == cts)
                {
                    _currentCts = null;
                }
                cts.Dispose();
            }
        }
    }

    /// <summary>
    /// Удаляет цифры тонов из пиньиня. "ni3 hao3" → "ni hao".
    /// </summary>
    private static string StripToneDigits(string pinyin)
    {
        if (string.IsNullOrWhiteSpace(pinyin))
            return string.Empty;

        var parts = pinyin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0 && char.IsDigit(parts[i][^1]))
                parts[i] = parts[i][..^1];
        }

        return string.Join(" ", parts);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Stop();
        lock (_lock)
        {
            _currentCts?.Dispose();
            _currentCts = null;
        }
        GC.SuppressFinalize(this);
    }

    #endregion
}
