using ChineseVocab.Services;
using ChineseVocab.ViewModels;

namespace ChineseVocab;

[QueryProperty(nameof(CharacterParam), "character")]
public partial class SentencesPage : ContentPage
{
    private SentencesViewModel? _viewModel;
    public string? CharacterParam { get; set; }

    public SentencesPage()
    {
        InitializeComponent();

        var sentenceService = Application.Current?.Handler?.MauiContext?.Services
            .GetRequiredService<ISentenceService>();
        _viewModel = new SentencesViewModel(sentenceService);
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (_viewModel is null) return;

        // Если страница открыта с параметром character — контекстный режим
        if (!string.IsNullOrEmpty(CharacterParam))
            _viewModel.ContextCharacter = CharacterParam;

        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
