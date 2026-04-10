using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChineseVocab.ViewModels
{
    /// <summary>
    /// ViewModel для страницы диктанта.
    /// Позволяет пользователю практиковать написание китайских иероглифов под диктовку.
    /// </summary>
    public partial class DictationViewModel : BaseViewModel
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// Текст для диктовки (иероглиф или слово).
        /// </summary>
        [ObservableProperty]
        private string _dictationText = string.Empty;

        /// <summary>
        /// Пиньинь для текста диктовки.
        /// </summary>
        [ObservableProperty]
        private string _pinyinHint = string.Empty;

        /// <summary>
        /// Перевод или определение текста диктовки.
        /// </summary>
        [ObservableProperty]
        private string _definitionHint = string.Empty;

        /// <summary>
        /// Введенный пользователем текст.
        /// </summary>
        [ObservableProperty]
        private string _userInput = string.Empty;

        /// <summary>
        /// Флаг, показывающий, проверен ли ответ.
        /// </summary>
        [ObservableProperty]
        private bool _isAnswerChecked = false;

        /// <summary>
        /// Флаг, указывающий, правильный ли ответ.
        /// </summary>
        [ObservableProperty]
        private bool _isAnswerCorrect = false;

        /// <summary>
        /// Сообщение с результатом проверки.
        /// </summary>
        [ObservableProperty]
        private string _checkResultMessage = string.Empty;

        /// <summary>
        /// Прогресс диктанта в текущей сессии (от 0 до 1).
        /// </summary>
        [ObservableProperty]
        private double _progress = 0.0;

        /// <summary>
        /// Текстовое представление прогресса (например, "2/10").
        /// </summary>
        [ObservableProperty]
        private string _progressText = "0/0";

        /// <summary>
        /// Количество правильно написанных иероглифов в текущей сессии.
        /// </summary>
        [ObservableProperty]
        private int _correctCount = 0;

        /// <summary>
        /// Общее количество заданий в текущей сессии.
        /// </summary>
        [ObservableProperty]
        private int _totalCount = 0;

        /// <summary>
        /// Конструктор ViewModel.
        /// </summary>
        public DictationViewModel()
        {
            Title = "Диктант";
            // Инициализируем тестовые данные
            LoadNextDictationItem();
        }

        /// <summary>
        /// Команда для начала новой сессии диктанта.
        /// </summary>
        public ICommand StartNewSessionCommand => new AsyncRelayCommand(StartNewSessionAsync);

        /// <summary>
        /// Команда для проверки введенного пользователем текста.
        /// </summary>
        public ICommand CheckAnswerCommand => new RelayCommand(CheckAnswer);

        /// <summary>
        /// Команда для перехода к следующему заданию.
        /// </summary>
        public ICommand NextItemCommand => new RelayCommand(NextItem);

        /// <summary>
        /// Команда для показа подсказки (пиньинь и перевод).
        /// </summary>
        public ICommand ShowHintCommand => new AsyncRelayCommand(ShowHintAsync);

        /// <summary>
        /// Команда для открытия настроек.
        /// </summary>
        public new ICommand OpenSettingsCommand => new AsyncRelayCommand(OpenSettingsAsync);

        /// <summary>
        /// Команда для возврата назад.
        /// </summary>
        public new ICommand GoBackCommand => new AsyncRelayCommand(GoBackAsync);

        private async Task StartNewSessionAsync()
        {
            IsBusy = true;

            try
            {
                // Сброс состояния
                CorrectCount = 0;
                TotalCount = 5; // Временное значение для демонстрации
                UpdateProgress();

                // Временная задержка для имитации загрузки
                await Task.Delay(500);

                // Загрузка первого задания
                LoadNextDictationItem();
                ResetCheckState();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CheckAnswer()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
            {
                CheckResultMessage = "Пожалуйста, введите ответ";
                return;
            }

            // Простая проверка: сравниваем введенный текст с текстом диктовки
            IsAnswerCorrect = UserInput.Trim() == DictationText;
            IsAnswerChecked = true;

            if (IsAnswerCorrect)
            {
                CheckResultMessage = "Правильно! 🎉";
                CorrectCount++;
            }
            else
            {
                CheckResultMessage = $"Неверно. Правильный ответ: {DictationText}";
            }

            UpdateProgress();
        }

        private void NextItem()
        {
            LoadNextDictationItem();
            ResetCheckState();
        }

        private async Task ShowHintAsync()
        {
            await Shell.Current.DisplayAlertAsync("Подсказка",
                $"Пиньинь: {PinyinHint}\nПеревод: {DefinitionHint}",
                "OK");
        }

        /// <summary>
        /// Загружает следующее задание для диктанта.
        /// Временная реализация с тестовыми данными.
        /// </summary>
        private void LoadNextDictationItem()
        {
            // Тестовые данные для демонстрации
            var dictationItems = new[]
            {
                new { Character = "人", Pinyin = "rén", Definition = "человек" },
                new { Character = "好", Pinyin = "hǎo", Definition = "хороший" },
                new { Character = "学", Pinyin = "xué", Definition = "учиться" },
                new { Character = "中", Pinyin = "zhōng", Definition = "середина, Китай" },
                new { Character = "大", Pinyin = "dà", Definition = "большой" },
            };

            var item = dictationItems[_random.Next(dictationItems.Length)];
            DictationText = item.Character;
            PinyinHint = item.Pinyin;
            DefinitionHint = item.Definition;
        }

        /// <summary>
        /// Сбрасывает состояние проверки ответа.
        /// </summary>
        private void ResetCheckState()
        {
            UserInput = string.Empty;
            IsAnswerChecked = false;
            IsAnswerCorrect = false;
            CheckResultMessage = string.Empty;
        }

        /// <summary>
        /// Обновляет прогресс диктанта.
        /// </summary>
        private void UpdateProgress()
        {
            if (TotalCount <= 0)
            {
                Progress = 0;
                ProgressText = "0/0";
                return;
            }

            Progress = (double)CorrectCount / TotalCount;
            ProgressText = $"{CorrectCount}/{TotalCount}";
        }

        /// <summary>
        /// Метод инициализации ViewModel при появлении страницы.
        /// </summary>
        public override async Task InitializeAsync()
        {
            IsBusy = true;

            try
            {
                // Здесь будет загрузка реальных данных из базы
                // await LoadDictationSessionAsync();

                // Временная задержка для имитации загрузки
                await Task.Delay(300);

                // Инициализация сессии
                CorrectCount = 0;
                TotalCount = 5;
                UpdateProgress();

                // Загрузка первого задания
                LoadNextDictationItem();
                ResetCheckState();
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Метод очистки ресурсов при скрытии страницы.
        /// </summary>
        public override Task OnDisappearingAsync()
        {
            // Сохраняем прогресс сессии
            // await SaveDictationSessionAsync();
            return Task.CompletedTask;
        }
    }
}
