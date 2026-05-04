using System.Text.Json;

namespace ChineseVocab.Services.Character;

/// <summary>
/// Загружает данные порядка черт из встроенного JSON-файла
/// и предоставляет кэшированный доступ к ним.
/// Для иероглифов, отсутствующих в JSON, генерирует заглушку на основе данных из БД.
/// </summary>
public class StrokeOrderDataLoader
{
    private Dictionary<string, StrokeOrderData>? _cache;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    /// <summary>
    /// Загружает данные порядка черт для указанного иероглифа.
    /// </summary>
    /// <param name="character">Китайский иероглиф (один символ или слово).</param>
    /// <returns>Данные порядка черт или null, если не найдены.</returns>
    public async Task<StrokeOrderData?> GetStrokeOrderAsync(string character)
    {
        await EnsureInitializedAsync();

        if (string.IsNullOrEmpty(character))
            return null;

        // Ищем точное совпадение
        if (_cache!.TryGetValue(character, out var data))
            return data;

        // Для слов из нескольких иероглифов возвращаем null
        // (порядок черт имеет смысл только для отдельных иероглифов)
        if (character.Length > 1)
            return null;

        return null; // Не найдено — вызывающий код сгенерирует заглушку
    }

    /// <summary>
    /// Проверяет, есть ли данные для указанного иероглифа.
    /// </summary>
    public async Task<bool> HasDataAsync(string character)
    {
        await EnsureInitializedAsync();
        return _cache!.ContainsKey(character);
    }

    /// <summary>
    /// Возвращает количество иероглифов с известным порядком черт.
    /// </summary>
    public async Task<int> GetCharacterCountAsync()
    {
        await EnsureInitializedAsync();
        return _cache!.Count;
    }

    /// <summary>
    /// Возвращает все известные иероглифы.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetKnownCharactersAsync()
    {
        await EnsureInitializedAsync();
        return _cache!.Keys.ToList().AsReadOnly();
    }

    #region Инициализация

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
                return;

            _cache = await LoadFromResourceAsync();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task<Dictionary<string, StrokeOrderData>> LoadFromResourceAsync()
    {
        var result = new Dictionary<string, StrokeOrderData>(StringComparer.Ordinal);

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("StrokeOrder/stroke_data.json");
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();

            var root = JsonSerializer.Deserialize<StrokeDataRoot>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (root?.Characters is not null)
            {
                foreach (var entry in root.Characters)
                {
                    if (!string.IsNullOrEmpty(entry.Character))
                    {
                        var strokeData = ToStrokeOrderData(entry);
                        result[entry.Character] = strokeData;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine(
                $"StrokeOrderDataLoader: загружено {result.Count} иероглифов");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"StrokeOrderDataLoader: ошибка загрузки — {ex.Message}");
        }

        return result;
    }

    private static StrokeOrderData ToStrokeOrderData(StrokeDataEntry entry)
    {
        return new StrokeOrderData
        {
            Character = entry.Character ?? string.Empty,
            TotalStrokes = entry.TotalStrokes,
            Rules = entry.Rules ?? string.Empty,
            CommonMistakes = entry.CommonMistakes ?? string.Empty,
            Strokes = entry.Strokes?.Select(s => new Services.Stroke
            {
                Number = s.Number,
                Type = s.Type ?? string.Empty,
                Direction = s.Direction ?? string.Empty,
                Points = s.Points?.Select(p => new Services.Point
                {
                    X = p.X,
                    Y = p.Y
                }).ToList() ?? new List<Services.Point>()
            }).ToList() ?? new List<Services.Stroke>()
        };
    }

    #endregion

    #region Модели десериализации

    private class StrokeDataRoot
    {
        public List<StrokeDataEntry>? Characters { get; set; }
    }

    private class StrokeDataEntry
    {
        public string? Character { get; set; }
        public int TotalStrokes { get; set; }
        public string? Rules { get; set; }
        public string? CommonMistakes { get; set; }
        public List<StrokeEntry>? Strokes { get; set; }
    }

    private class StrokeEntry
    {
        public int Number { get; set; }
        public string? Type { get; set; }
        public string? Direction { get; set; }
        public List<PointEntry>? Points { get; set; }
    }

    private class PointEntry
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    #endregion
}
