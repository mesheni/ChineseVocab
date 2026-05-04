using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChineseVocab.Models;
using ChineseVocab.Services;
using ChineseVocab.Utils;

namespace ChineseVocab.ViewModels;

/// <summary>
/// ViewModel для страницы библиотеки иероглифов (БКРС).
/// Обеспечивает поиск, фильтрацию и группировку иероглифов.
/// </summary>
public partial class CharacterLibraryViewModel : BaseViewModel
{
    private readonly ICharacterService _characterService;
    private CancellationTokenSource? _searchCts;

    // Все загруженные карточки (без фильтрации)
    private List<Card> _allCards = [];

    public CharacterLibraryViewModel(ICharacterService characterService)
    {
        _characterService = characterService;
        Title = "База иероглифов";
    }

    #region Observable свойства

    [ObservableProperty]
    private ObservableCollection<CardGroup> _cardGroups = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _resultSummary = string.Empty;

    // Фильтры
    [ObservableProperty]
    private int _selectedHskLevel = 0; // 0 = все

    [ObservableProperty]
    private string _selectedCharacterType = string.Empty; // "" = все

    [ObservableProperty]
    private string _selectedRadical = string.Empty;

    [ObservableProperty]
    private int _minStrokes;

    [ObservableProperty]
    private int _maxStrokes = 50;

    [ObservableProperty]
    private SortMode _currentSort = SortMode.ByHsk;

    // Списки для пикеров
    public List<int> HskLevels { get; } = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
    public List<string> CharacterTypes { get; } = ["", "Пиктограммы", "Указательные", "Идеограммы", "Фоноидеограммы", "Заимствованные", "Производные"];
    public List<string> SortModes { get; } = ["По HSK", "По частоте", "По чертам"];
    public List<string> Radicals { get; private set; } = [];

    // Выбранная карточка для навигации
    [ObservableProperty]
    private Card? _selectedCard;

    #endregion

    #region Команды

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Загружаем радикалы для фильтра
            try { Radicals = await _characterService.GetAllRadicalsAsync(); }
            catch { Radicals = []; }

            // Загружаем все карточки
            _allCards = await _characterService.SearchCharactersAsync("");
            TotalCount = _allCards.Count;

            ApplyFiltersAndSort();
            IsBusy = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CharLib init error: {ex.Message}");
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        // Дебаунс: отменяем предыдущий поиск
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        await Task.Delay(300, token); // дебаунс 300ms

        if (token.IsCancellationRequested) return;

        try
        {
            IsSearching = true;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                _allCards = await _characterService.SearchCharactersAsync("");
            }
            else
            {
                _allCards = await _characterService.SearchCharactersAsync(SearchText.Trim());
            }

            TotalCount = _allCards.Count;
            ApplyFiltersAndSort();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    /// <summary>
    /// Вызывается при изменении SearchText из UI (с debounce).
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        _ = SearchAsync();
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        ApplyFiltersAndSort();
    }

    partial void OnSelectedHskLevelChanged(int value) => ApplyFiltersAndSort();
    partial void OnSelectedCharacterTypeChanged(string value) => ApplyFiltersAndSort();
    partial void OnMinStrokesChanged(int value) => ApplyFiltersAndSort();
    partial void OnMaxStrokesChanged(int value) => ApplyFiltersAndSort();

    [RelayCommand]
    private void ChangeSort(string mode)
    {
        CurrentSort = mode switch
        {
            "По HSK" => SortMode.ByHsk,
            "По частоте" => SortMode.ByFrequency,
            "По чертам" => SortMode.ByStrokes,
            _ => SortMode.ByHsk
        };
        ApplyFiltersAndSort();
    }

    [RelayCommand]
    private async Task SelectCardAsync(Card? card)
    {
        if (card is null) return;
        SelectedCard = card;
        await Shell.Current.GoToAsync($"characterDetail?character={Uri.EscapeDataString(card.Character)}");
        SelectedCard = null;
    }

    #endregion

    #region Фильтрация и сортировка

    private void ApplyFiltersAndSort()
    {
        var filtered = _allCards.AsEnumerable();

        // Фильтр по HSK
        if (SelectedHskLevel > 0)
            filtered = filtered.Where(c => c.HskLevel == SelectedHskLevel);

        // Фильтр по типу
        if (!string.IsNullOrEmpty(SelectedCharacterType))
            filtered = filtered.Where(c =>
                c.CharacterType.Contains(SelectedCharacterType, StringComparison.OrdinalIgnoreCase));

        // Фильтр по радикалу
        if (!string.IsNullOrEmpty(SelectedRadical))
            filtered = filtered.Where(c =>
                c.Radical.Contains(SelectedRadical, StringComparison.OrdinalIgnoreCase));

        // Фильтр по чертам
        filtered = filtered.Where(c => c.StrokeCount >= MinStrokes && c.StrokeCount <= MaxStrokes);

        // Сортировка
        var sorted = CurrentSort switch
        {
            SortMode.ByFrequency => filtered.OrderBy(c => c.FrequencyRank),
            SortMode.ByStrokes => filtered.OrderBy(c => c.StrokeCount),
            _ => filtered.OrderBy(c => c.HskLevel).ThenBy(c => c.StrokeCount)
        };

        var list = sorted.ToList();
        TotalCount = list.Count;
        ResultSummary = $"Найдено: {TotalCount}";

        // Группировка по HSK
        var groups = list
            .GroupBy(c => c.HskLevel)
            .OrderBy(g => g.Key)
            .Select(g => new CardGroup(
                g.Key == 0 ? "Вне HSK" : $"HSK {g.Key}",
                g.Key,
                [.. g]
            ))
            .ToList();

        CardGroups = new ObservableCollection<CardGroup>(groups);
    }

    #endregion
}

/// <summary>
/// Группа карточек для отображения в CollectionView с группировкой.
/// </summary>
public class CardGroup : ObservableCollection<Card>
{
    public string GroupName { get; }
    public int HskLevel { get; }
    public int Count => Items.Count;

    public CardGroup(string groupName, int hskLevel, IEnumerable<Card> cards)
        : base(cards)
    {
        GroupName = groupName;
        HskLevel = hskLevel;
    }
}

/// <summary>
/// Режим сортировки.
/// </summary>
public enum SortMode
{
    ByHsk,
    ByFrequency,
    ByStrokes
}
