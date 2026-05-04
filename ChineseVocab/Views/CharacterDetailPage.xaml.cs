using ChineseVocab.Models;
using ChineseVocab.Services;
using ChineseVocab.Services.Audio;
using ChineseVocab.Utils;

namespace ChineseVocab.Views;

[QueryProperty(nameof(CharacterParam), "character")]
public partial class CharacterDetailPage : ContentPage
{
    private readonly ICharacterService _characterService;
    private readonly ITextToSpeechService? _tts;
    private Card? _card;

    public string? CharacterParam { get; set; }

    public CharacterDetailPage()
    {
        InitializeComponent();

        _characterService = Application.Current?.Handler?.MauiContext?.Services
            .GetRequiredService<ICharacterService>();
        _tts = Application.Current?.Handler?.MauiContext?.Services
            .GetService<ITextToSpeechService>();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (string.IsNullOrEmpty(CharacterParam))
            return;

        await LoadCharacterAsync(CharacterParam);
    }

    private async Task LoadCharacterAsync(string character)
    {
        try
        {
            _card = await _characterService.GetCharacterBySymbolAsync(character);
            if (_card is null)
            {
                await DisplayAlert("Ошибка", $"Иероглиф '{character}' не найден.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            UpdateUI(_card);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DetailPage error: {ex.Message}");
        }
    }

    private void UpdateUI(Card card)
    {
        Title = card.Character;
        CharacterLabel.Text = card.Character;

        // Пиньинь с цветными тонами
        string displayPinyin = card.Pinyin;
        try
        {
            // Конвертируем цифровые тоны в диакритические
            string diacriticPinyin = PinyinConverter.NumberedToDiacritics(card.Pinyin);
            if (!string.IsNullOrWhiteSpace(diacriticPinyin))
            {
                displayPinyin = diacriticPinyin;
                PinyinLabel.FormattedText = PinyinConverter.FormatWithToneColors(diacriticPinyin);
            }
            else
            {
                PinyinLabel.Text = card.Pinyin;
            }
        }
        catch
        {
            PinyinLabel.Text = card.Pinyin;
        }

        DefinitionLabel.Text = card.Definition;

        // Тип иероглифа
        if (!string.IsNullOrEmpty(card.CharacterType))
        {
            TypeNameLabel.Text = card.CharacterType;
            TypeDescriptionLabel.Text = GetTypeDescription(card.CharacterType);

            var (icon, colorHex) = GetTypeVisual(card.CharacterType);
            TypeIconLabel.Text = icon;
            TypeBadge.BackgroundColor = Color.FromArgb(colorHex);
        }
        else
        {
            TypeNameLabel.Text = "Неизвестный тип";
            TypeIconLabel.Text = "❓";
            TypeBadge.BackgroundColor = Color.FromArgb("#9E9E9E");
        }

        // HSK уровень
        HskLabel.Text = card.HskLevel > 0 ? card.HskLevel.ToString() : "—";

        // Черты
        StrokeCountLabel.Text = card.StrokeCount > 0 ? card.StrokeCount.ToString() : "?";

        // Радикал
        RadicalLabel.Text = !string.IsNullOrEmpty(card.Radical) ? card.Radical : "?";

        // Частота
        FrequencyLabel.Text = card.FrequencyRank > 0
            ? $"#{card.FrequencyRank:N0}"
            : "—";

        // Компоненты
        if (!string.IsNullOrEmpty(card.Components))
        {
            ComponentsFrame.IsVisible = true;
            ComponentsLabel.Text = card.Components;
        }

        // Заметки
        if (!string.IsNullOrEmpty(card.Notes))
        {
            NotesFrame.IsVisible = true;
            NotesLabel.Text = card.Notes;
        }
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (_card?.Character is not string ch || string.IsNullOrEmpty(ch) || _tts is null)
            return;

        try { await _tts.SpeakChineseAsync(ch); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"TTS: {ex.Message}"); }
    }

    private async void OnStrokeOrderClicked(object? sender, EventArgs e)
    {
        if (_card?.Character is not string ch || string.IsNullOrEmpty(ch))
            return;

        await Shell.Current.GoToAsync(
            $"strokeOrder?character={Uri.EscapeDataString(ch)}");
    }

    private async void OnExamplesClicked(object? sender, EventArgs e)
    {
        if (_card?.Character is not string ch || string.IsNullOrEmpty(ch))
            return;

        await Shell.Current.GoToAsync(
            $"sentences?character={Uri.EscapeDataString(ch)}");
    }

    private static string GetTypeDescription(string typeName)
    {
        return typeName switch
        {
            "Пиктограммы" => "Изображает конкретный предмет. Самый древний тип иероглифов.",
            "Указательные" => "Указывает на абстрактное понятие или положение.",
            "Идеограммы" => "Объединяет значения нескольких компонентов в новый смысл.",
            "Фоноидеограммы" => "Состоит из смыслового компонента (радикала) и фонетика (звучания). Самый распространённый тип (80-90%).",
            "Заимствованные" => "Первоначально создан для одного значения, позже заимствован для другого.",
            "Производные" => "Образован изменением существующего иероглифа для родственного понятия.",
            _ => "Классификация иероглифа согласно традиционной системе."
        };
    }

    private static (string icon, string colorHex) GetTypeVisual(string typeName)
    {
        return typeName switch
        {
            "Пиктограммы" => ("🖼️", "#4CAF50"),
            "Указательные" => ("☝️", "#2196F3"),
            "Идеограммы" => ("💡", "#FF9800"),
            "Фоноидеограммы" => ("🎵", "#9C27B0"),
            "Заимствованные" => ("🔀", "#F44336"),
            "Производные" => ("🔄", "#607D8B"),
            _ => ("❓", "#9E9E9E")
        };
    }
}
