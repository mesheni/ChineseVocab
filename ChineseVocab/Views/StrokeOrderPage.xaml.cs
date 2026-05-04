using ChineseVocab.Services;
using ChineseVocab.ViewModels;

namespace ChineseVocab.Views;

[QueryProperty(nameof(Character), "character")]
public partial class StrokeOrderPage : ContentPage
{
    public string? Character { get; set; }

    public StrokeOrderPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (!string.IsNullOrEmpty(Character))
        {
            var characterService = Application.Current?.Handler?.MauiContext?.Services
                .GetService<ICharacterService>();
            StrokeOrderControl.BindingContext = new StrokeOrderViewModel(characterService!, Character);
        }
    }
}
