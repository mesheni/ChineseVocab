using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChineseVocab.Models;

namespace ChineseVocab.ViewModels
{
    /// <summary>
    /// ViewModel для страницы изучения карточек с системой интервальных повторений (SRS).
    /// </summary>
    public partial class StudyViewModel : BaseViewModel
    {
        private readonly Random _random = new Random();
        private int _currentCardIndex = 0;
        private int _totalCardsStudied = 0;
        private int _totalCardsInSession = 10; // Временное значение для демонстрации

        /// <summary>
        /// Текущая карточка, отображаемая пользователю.
        /// </summary>
        [ObservableProperty]
        private Card _currentCard = default!;

        /// <summary>
        /// Флаг, указывающий, виден ли ответ (пиньинь и перевод).
        /// </summary>
        [ObservableProperty]
        private bool _isAnswerVisible = false;

        /// <summary>
        /// Прогресс изучения в текущей сессии (от 0 до 1).
        /// </summary>
        [ObservableProperty]
        private double _progress = 0.0;

        /// <summary>
        /// Текстовое представление прогресса (например, "3/10").
        /// </summary>
        [ObservableProperty]
        private string _progressText = "0/0";

        /// <summary>
        /// Флаг, указывающий, есть ли примеры предложений для текущей карточки.
        /// </summary>
        [ObservableProperty]
        private bool _hasExamples = false;

        /// <summary>
        /// Текст примеров предложений для текущей карточки.
        /// </summary>
        [ObservableProperty]
        private string _examplesText = string.Empty;

        /// <summary>
        /// Коллекция карточек для текущей сессии изучения.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Card> _cards = new ObservableCollection<Card>();

        /// <summary>
        /// Конструктор ViewModel.
        /// </summary>
        public StudyViewModel()
        {
            Title = "Изучение карточек";
            // Инициализируем тестовые данные
            InitializeTestCards();
            LoadNextCard();
        }

        /// <summary>
        /// Команда для показа ответа (пиньинь и перевода).
        /// </summary>
        [RelayCommand]
        private void ShowAnswer()
        {
            IsAnswerVisible = true;
            UpdateExamples();
        }

        /// <summary>
        /// Команда для оценки карточки пользователем (оценка от 1 до 5).
        /// Вызывает алгоритм SRS для расчета следующего интервала повторения.
        /// </summary>
        [RelayCommand]
        private async Task RateCardAsync(int rating)
        {
            if (rating < 1 || rating > 5)
                return;

            IsBusy = true;

            try
            {
                // Здесь будет вызов SRS движка для обработки оценки
                // ProcessCardWithSRS(CurrentCard, rating);

                // Логирование для отладки
                Console.WriteLine($"Card rated: {CurrentCard.Character}, Rating: {rating}");

                // Обновляем статистику
                _totalCardsStudied++;
                UpdateProgress();

                // Загружаем следующую карточку
                await Task.Delay(300); // Небольшая задержка для плавности
                LoadNextCard();

                // Сбрасываем видимость ответа для новой карточки
                IsAnswerVisible = false;
                HasExamples = false;
                ExamplesText = string.Empty;
            }
            finally
            {
                IsBusy = false;
            }
        }



        /// <summary>
        /// Метод для загрузки следующей карточки.
        /// </summary>
        private void LoadNextCard()
        {
            if (Cards.Count == 0)
            {
                CurrentCard = new Card("无", "wú", "нет, не иметь", 3);
                return;
            }

            // Простой алгоритм выбора следующей карточки
            // В будущем будет заменен на алгоритм SRS с приоритетом карточек для повторения
            _currentCardIndex = (_currentCardIndex + 1) % Cards.Count;
            CurrentCard = Cards[_currentCardIndex];
        }

        /// <summary>
        /// Обновление примеров предложений для текущей карточки.
        /// </summary>
        private void UpdateExamples()
        {
            // Временная реализация - возвращаем тестовые примеры
            // В будущем примеры будут загружаться из базы данных
            if (CurrentCard == null)
            {
                HasExamples = false;
                return;
            }

            // Примеры для некоторых иероглифов
            switch (CurrentCard.Character)
            {
                case "人":
                    HasExamples = true;
                    ExamplesText = "我是一个人。 (Wǒ shì yī gè rén.) - Я человек.";
                    break;
                case "好":
                    HasExamples = true;
                    ExamplesText = "你好！ (Nǐ hǎo!) - Привет!\n很好 (Hěn hǎo) - Очень хорошо";
                    break;
                case "学":
                    HasExamples = true;
                    ExamplesText = "学习中文 (Xuéxí Zhōngwén) - Учить китайский язык\n学校 (Xuéxiào) - Школа";
                    break;
                default:
                    HasExamples = false;
                    ExamplesText = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Обновление прогресса изучения.
        /// </summary>
        private void UpdateProgress()
        {
            if (_totalCardsInSession <= 0)
            {
                Progress = 0;
                ProgressText = "0/0";
                return;
            }

            Progress = (double)_totalCardsStudied / _totalCardsInSession;
            ProgressText = $"{_totalCardsStudied}/{_totalCardsInSession}";
        }

        /// <summary>
        /// Инициализация тестовых карточек для демонстрации.
        /// В будущем будет заменена на загрузку из базы данных.
        /// </summary>
        private void InitializeTestCards()
        {
            Cards.Clear();

            // Добавляем тестовые карточки (базовые иероглифы HSK 1)
            Cards.Add(new Card("人", "rén", "человек", 1) { StrokeCount = 2, Radical = "人" });
            Cards.Add(new Card("好", "hǎo", "хороший", 1) { StrokeCount = 6, Radical = "女" });
            Cards.Add(new Card("学", "xué", "учиться", 1) { StrokeCount = 8, Radical = "子" });
            Cards.Add(new Card("中", "zhōng", "середина, Китай", 1) { StrokeCount = 4, Radical = "丨" });
            Cards.Add(new Card("国", "guó", "страна", 1) { StrokeCount = 8, Radical = "囗" });
            Cards.Add(new Card("大", "dà", "большой", 1) { StrokeCount = 3, Radical = "大" });
            Cards.Add(new Card("小", "xiǎo", "маленький", 1) { StrokeCount = 3, Radical = "小" });
            Cards.Add(new Card("上", "shàng", "верх, на", 1) { StrokeCount = 3, Radical = "一" });
            Cards.Add(new Card("下", "xià", "низ, под", 1) { StrokeCount = 3, Radical = "一" });
            Cards.Add(new Card("我", "wǒ", "я", 1) { StrokeCount = 7, Radical = "戈" });

            // Перемешиваем карточки для разнообразия
            var shuffled = Cards.OrderBy(x => _random.Next()).ToList();
            Cards.Clear();
            foreach (var card in shuffled)
            {
                Cards.Add(card);
            }

            _totalCardsInSession = Cards.Count;
            UpdateProgress();
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
                // await LoadStudySessionAsync();

                // Временная задержка для имитации загрузки
                await Task.Delay(500);

                // Обновляем прогресс
                UpdateProgress();
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
            // await SaveStudySessionAsync();
            return Task.CompletedTask;
        }
    }
}
