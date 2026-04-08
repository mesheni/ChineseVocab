using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChineseVocab.ViewModels
{
    /// <summary>
    /// ViewModel для главной страницы приложения.
    /// </summary>
    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _welcomeMessage = "Добро пожаловать в ChineseVocab!";

        [ObservableProperty]
        private string _userName = string.Empty;

        /// <summary>
        /// Команда для перехода к модулю изучения карточек.
        /// </summary>
        [RelayCommand]
        private async Task GoToStudyAsync()
        {
            await Shell.Current.GoToAsync("StudyPage");
        }

        /// <summary>
        /// Команда для перехода к модулю диктанта.
        /// </summary>
        [RelayCommand]
        private async Task GoToDictationAsync()
        {
            await Shell.Current.GoToAsync("DictationPage");
        }

        /// <summary>
        /// Команда для перехода к базе иероглифов (БКРС).
        /// </summary>
        [RelayCommand]
        private async Task GoToCharacterLibraryAsync()
        {
            await Shell.Current.GoToAsync("CharacterLibraryPage");
        }

        /// <summary>
        /// Команда для перехода к модулю предложений.
        /// </summary>
        [RelayCommand]
        private async Task GoToSentencesAsync()
        {
            await Shell.Current.GoToAsync("SentencesPage");
        }

        /// <summary>
        /// Команда для перехода к статистике.
        /// </summary>
        [RelayCommand]
        private async Task GoToStatisticsAsync()
        {
            await Shell.Current.GoToAsync("StatisticsPage");
        }

        /// <summary>
        /// Метод инициализации ViewModel.
        /// </summary>
        public override async Task InitializeAsync()
        {
            IsBusy = true;

            try
            {
                // Здесь можно загрузить данные пользователя, например имя
                // UserName = await UserService.GetUserNameAsync();
                WelcomeMessage = $"你好, {UserName}!"; // Приветствие на китайском
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
