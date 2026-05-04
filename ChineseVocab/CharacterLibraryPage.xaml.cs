using ChineseVocab.Converters;
using ChineseVocab.Services;
using ChineseVocab.ViewModels;

namespace ChineseVocab;

public partial class CharacterLibraryPage : ContentPage
{
    private CharacterLibraryViewModel? _viewModel;

    public CharacterLibraryPage()
    {
        InitializeComponent();

        var characterService = Application.Current?.Handler?.MauiContext?.Services
            .GetRequiredService<ICharacterService>();
        _viewModel = new CharacterLibraryViewModel(characterService);
        BindingContext = _viewModel;

        // Добавляем конвертер в ресурсы страницы
        Resources.Add("IntToBoolConverter", new IntToBoolConverter());

        // Обработчик изменения сортировки
        SortPicker.SelectedIndexChanged += OnSortChanged;
    }

    private void OnSortChanged(object? sender, EventArgs e)
    {
        if (SortPicker.SelectedItem is string mode)
            _viewModel?.ChangeSortCommand.Execute(mode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel is not null)
            await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
