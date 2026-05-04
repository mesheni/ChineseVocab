using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChineseVocab.Models;
using ChineseVocab.Services;
using ChineseVocab.Utils;

namespace ChineseVocab.ViewModels;

/// <summary>
/// ViewModel для страницы примеров предложений.
/// Поддерживает глобальный режим (все предложения) и контекстный режим (для иероглифа).
/// </summary>
public partial class SentencesViewModel : BaseViewModel
{
    private readonly ISentenceService _sentenceService;
    private CancellationTokenSource? _searchCts;

    private List<Sentence> _allSentences = [];

    public SentencesViewModel(ISentenceService sentenceService)
    {
        _sentenceService = sentenceService;
        Title = "Примеры предложений";
    }

    #region Observable свойства

    [ObservableProperty]
    private ObservableCollection<SentenceDisplay> _sentences = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _resultSummary = string.Empty;

    [ObservableProperty]
    private string? _contextCharacter;

    // Фильтры
    [ObservableProperty]
    private int _selectedDifficulty; // 0 = все

    [ObservableProperty]
    private string _selectedSource = string.Empty;

    // Раскрытие перевода
    [ObservableProperty]
    private SentenceDisplay? _expandedSentence;

    // Списки для пикеров
    public List<int> DifficultyLevels { get; } = [0, 1, 2, 3, 4, 5];
    public List<string> Sources { get; private set; } = ["", "HSK", "Учебник", "Разговорный", "Литература"];

    #endregion

    #region Команды

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            if (!string.IsNullOrEmpty(ContextCharacter))
            {
                var sentences = await _sentenceService.GetSentencesByCharacterAsync(ContextCharacter);
                _allSentences = sentences;
                Title = $"Примеры: {ContextCharacter}";
            }
            else
            {
                _allSentences = await _sentenceService.GetAllSentencesAsync();
                Title = "Примеры предложений";
            }

            // Собираем уникальные источники
            var srcs = _allSentences
                .Select(s => s.Source)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            if (srcs.Count > 0)
                Sources = ["", .. srcs];

            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sentences init error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        await Task.Delay(300, token);
        if (token.IsCancellationRequested) return;

        try
        {
            IsSearching = true;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                if (!string.IsNullOrEmpty(ContextCharacter))
                    _allSentences = await _sentenceService.GetSentencesByCharacterAsync(ContextCharacter);
                else
                    _allSentences = await _sentenceService.GetAllSentencesAsync();
            }
            else
            {
                _allSentences = await _sentenceService.SearchSentencesAsync(SearchText.Trim());
            }

            ApplyFilters();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"Sentences search error: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    partial void OnSearchTextChanged(string value) => _ = SearchAsync();

    [RelayCommand]
    private void ApplyFilters()
    {
        ApplyFiltersAndUpdate();
    }

    partial void OnSelectedDifficultyChanged(int value) => ApplyFiltersAndUpdate();
    partial void OnSelectedSourceChanged(string value) => ApplyFiltersAndUpdate();

    [RelayCommand]
    private void ToggleExpand(SentenceDisplay? sentence)
    {
        if (sentence is null) return;

        if (ExpandedSentence == sentence)
        {
            sentence.IsExpanded = false;
            ExpandedSentence = null;
        }
        else
        {
            if (ExpandedSentence is not null)
                ExpandedSentence.IsExpanded = false;

            sentence.IsExpanded = true;
            ExpandedSentence = sentence;
        }
    }

    [RelayCommand]
    private async Task SpeakSentenceAsync(SentenceDisplay? sentence)
    {
        if (sentence is null) return;

        var tts = Application.Current?.Handler?.MauiContext?.Services
            .GetService<Services.Audio.ITextToSpeechService>();
        if (tts is null) return;

        try
        {
            await tts.SpeakChineseAsync(sentence.ChineseText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TTS sentence: {ex.Message}");
        }
    }

    #endregion

    #region Фильтрация

    private void ApplyFiltersAndUpdate()
    {
        var filtered = _allSentences.AsEnumerable();

        if (SelectedDifficulty > 0)
            filtered = filtered.Where(s => s.DifficultyLevel == SelectedDifficulty);

        if (!string.IsNullOrEmpty(SelectedSource))
            filtered = filtered.Where(s =>
                s.Source.Equals(SelectedSource, StringComparison.OrdinalIgnoreCase));

        var list = filtered.ToList();
        ResultSummary = list.Count > 0
            ? $"Найдено: {list.Count} предложений"
            : "Предложения не найдены";
        IsEmpty = list.Count == 0;

        Sentences = new ObservableCollection<SentenceDisplay>(
            list.Select(s => new SentenceDisplay(s)));
    }

    #endregion
}

/// <summary>
/// Обёртка Sentence для UI с состоянием раскрытия и форматированным пиньинем.
/// </summary>
public partial class SentenceDisplay : ObservableObject
{
    private readonly Sentence _sentence;

    public int Id => _sentence.Id;
    public string ChineseText => _sentence.ChineseText;
    public string Translation => _sentence.Translation;
    public string Explanation => _sentence.Explanation;
    public int DifficultyLevel => _sentence.DifficultyLevel;
    public string Source => _sentence.Source;
    public int ViewCount => _sentence.ViewCount;

    public string Pinyin
    {
        get
        {
            try
            {
                string converted = PinyinConverter.NumberedToDiacritics(_sentence.Pinyin);
                return !string.IsNullOrWhiteSpace(converted) ? converted : _sentence.Pinyin;
            }
            catch { return _sentence.Pinyin; }
        }
    }

    public FormattedString FormattedPinyin
    {
        get
        {
            try
            {
                string pinyin = Pinyin;
                return PinyinConverter.FormatWithToneColors(pinyin);
            }
            catch
            {
                return new FormattedString();
            }
        }
    }

    [ObservableProperty]
    private bool _isExpanded;

    public string DifficultyLabel => DifficultyLevel switch
    {
        1 => "⭐",
        2 => "⭐⭐",
        3 => "⭐⭐⭐",
        4 => "⭐⭐⭐⭐",
        5 => "⭐⭐⭐⭐⭐",
        _ => "—"
    };

    public Color DifficultyColor => DifficultyLevel switch
    {
        1 => Color.FromArgb("#4CAF50"),
        2 => Color.FromArgb("#8BC34A"),
        3 => Color.FromArgb("#FF9800"),
        4 => Color.FromArgb("#F44336"),
        5 => Color.FromArgb("#9C27B0"),
        _ => Color.FromArgb("#9E9E9E")
    };

    public bool HasSource => !string.IsNullOrWhiteSpace(Source);
    public bool HasExplanation => !string.IsNullOrWhiteSpace(Explanation);

    public SentenceDisplay(Sentence sentence)
    {
        _sentence = sentence;
    }
}
