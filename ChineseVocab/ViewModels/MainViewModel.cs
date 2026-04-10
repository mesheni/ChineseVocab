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
        public ICommand GoToStudyCommand => new AsyncRelayCommand(GoToStudyAsync);

        /// <summary>
        /// Команда для перехода к модулю диктанта.
        /// </summary>
        public ICommand GoToDictationCommand => new AsyncRelayCommand(GoToDictationAsync);

        /// <summary>
        /// Команда для перехода к базе иероглифов (БКРС).
        /// </summary>
        public ICommand GoToCharacterLibraryCommand => new AsyncRelayCommand(GoToCharacterLibraryAsync);

        /// <summary>
        /// Команда для перехода к модулю предложений.
        /// </summary>
        public ICommand GoToSentencesCommand => new AsyncRelayCommand(GoToSentencesAsync);

        /// <summary>
        /// Команда для перехода к статистике.
        /// </summary>
        public ICommand GoToStatisticsCommand => new AsyncRelayCommand(GoToStatisticsAsync);

        private async Task GoToStudyAsync()
        {
            await Shell.Current.GoToAsync("///StudyPage");
        }

        /// <summary>
        /// Команда для перехода к модулю диктанта.
        /// </summary>
        private async Task GoToDictationAsync()
        {
            await Shell.Current.GoToAsync("///DictationPage");
        }

        /// <summary>
        /// Команда для перехода к базе иероглифов (БКРС).
        /// </summary>
        private async Task GoToCharacterLibraryAsync()
        {
            await Shell.Current.GoToAsync("///CharacterLibraryPage");
        }

        /// <summary>
        /// Команда для перехода к модулю предложений.
        /// </summary>
        private async Task GoToSentencesAsync()
        {
            await Shell.Current.GoToAsync("///SentencesPage");
        }

        /// <summary>
        /// Команда для перехода к статистике.
        /// </summary>
        private async Task GoToStatisticsAsync()
        {
            await Shell.Current.GoToAsync("///StatisticsPage");
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
