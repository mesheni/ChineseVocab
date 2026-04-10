using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChineseVocab.Services;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using SkiaSharp;

namespace ChineseVocab.ViewModels
{
    /// <summary>
    /// ViewModel для модуля отображения и практики порядка черт китайских иероглифов.
    /// </summary>
    public partial class StrokeOrderViewModel : ObservableObject
    {
        private readonly ICharacterService _characterService;
        private StrokeOrderData _strokeOrderData;
        private Timer _animationTimer;
        private readonly int _animationInterval = 50; // миллисекунды между кадрами
        private List<Services.Point> _userStrokePoints = new List<Services.Point>();
        private bool _isStrokeValidated;
        private string _initialCharacter;

        public event EventHandler AnimationUpdated;

        [ObservableProperty]
        private string _character = string.Empty;

        [ObservableProperty]
        private int _totalStrokes;

        [ObservableProperty]
        private string _strokeRules = string.Empty;

        [ObservableProperty]
        private string _commonMistakes = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isAnimating;

        [ObservableProperty]
        private double _animationProgress;

        [ObservableProperty]
        private int _currentStrokeNumber = 1;

        [ObservableProperty]
        private double _animationSpeed = 1.0;

        [ObservableProperty]
        private bool _showGrid = true;

        [ObservableProperty]
        private bool _isPracticeMode;

        [ObservableProperty]
        private bool _autoRepeat;

        [ObservableProperty]
        private StrokeDisplayMode _displayMode = StrokeDisplayMode.Sequential;

        [ObservableProperty]
        private Stroke _currentStroke;

        [ObservableProperty]
        private List<Stroke> _strokes;

        [ObservableProperty]
        private Size _canvasSize = new Size(300, 300);

        [ObservableProperty]
        private string _validationStatus = string.Empty;

        [ObservableProperty]
        private Color _validationColor = Colors.Transparent;

        /// <summary>
        /// Текст кнопки воспроизведения/паузы.
        /// </summary>
        public string PlayButtonText => IsAnimating ? "⏸" : "▶";

        /// <summary>
        /// Флаг последовательного режима отображения.
        /// </summary>
        public bool IsSequentialMode => DisplayMode == StrokeDisplayMode.Sequential;

        /// <summary>
        /// Флаг режима отображения всех черт.
        /// </summary>
        public bool IsAllStrokesMode => DisplayMode == StrokeDisplayMode.AllStrokes;

        /// <summary>
        /// Прогресс изучения (процент завершенных черт).
        /// </summary>
        public double PracticeProgress => TotalStrokes > 0 ? (CurrentStrokeNumber - 1) / (double)TotalStrokes : 0;

        #region Команды

        /// <summary>
        /// Команда загрузки данных о порядке черт.
        /// </summary>
        public IAsyncRelayCommand LoadDataCommand { get; private set; }

        /// <summary>
        /// Команда переключения анимации (воспроизведение/пауза).
        /// </summary>
        public IRelayCommand ToggleAnimationCommand { get; private set; }

        /// <summary>
        /// Команда остановки анимации.
        /// </summary>
        public IRelayCommand StopAnimationCommand { get; private set; }

        /// <summary>
        /// Команда перемотки к началу.
        /// </summary>
        public IRelayCommand RewindCommand { get; private set; }

        /// <summary>
        /// Команда перехода к следующей черте.
        /// </summary>
        public IRelayCommand StepForwardCommand { get; private set; }

        /// <summary>
        /// Команда сброса анимации.
        /// </summary>
        public IRelayCommand ResetAnimationCommand { get; private set; }

        /// <summary>
        /// Команда изменения режима отображения.
        /// </summary>
        public IRelayCommand<string> ChangeDisplayModeCommand { get; private set; }

        /// <summary>
        /// Команда начала практики написания.
        /// </summary>
        public IRelayCommand StartPracticeCommand { get; private set; }

        /// <summary>
        /// Команда закрытия/возврата.
        /// </summary>
        public IRelayCommand CloseCommand { get; private set; }

        /// <summary>
        /// Команда повторной загрузки при ошибке.
        /// </summary>
        public IAsyncRelayCommand RetryLoadCommand { get; private set; }

        #endregion

        public StrokeOrderViewModel()
        {
            // Временная инициализация для дизайн-тайма
            InitializeCommands();
            InitializeSampleData();
        }

        public StrokeOrderViewModel(ICharacterService characterService, string character)
        {
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
            _initialCharacter = character ?? throw new ArgumentNullException(nameof(character));
            Character = character;

            InitializeCommands();
            _ = LoadStrokeOrderDataAsync();
        }

        private void InitializeCommands()
        {
            LoadDataCommand = new AsyncRelayCommand(LoadStrokeOrderDataAsync);
            ToggleAnimationCommand = new RelayCommand(ToggleAnimation);
            StopAnimationCommand = new RelayCommand(StopAnimation);
            RewindCommand = new RelayCommand(Rewind);
            StepForwardCommand = new RelayCommand(StepForward);
            ResetAnimationCommand = new RelayCommand(ResetAnimation);
            ChangeDisplayModeCommand = new RelayCommand<string>(ChangeDisplayMode);
            StartPracticeCommand = new RelayCommand(StartPractice);
            CloseCommand = new RelayCommand(Close);
            RetryLoadCommand = new AsyncRelayCommand(LoadStrokeOrderDataAsync);
        }

        private void InitializeSampleData()
        {
            // Данные для дизайн-тайма
            Character = "永";
            TotalStrokes = 5;
            StrokeRules = "1. Слева направо, сверху вниз\n2. Горизонтальные перед вертикальными\n3. Сначала внешние, потом внутренние";
            CommonMistakes = "Неправильный порядок третьей и четвертой черты";

            Strokes = new List<Stroke>
            {
                new Stroke { Number = 1, Type = "dot", Points = new List<Services.Point> { new Services.Point { X = 50, Y = 20 } }, Direction = "top-to-bottom" },
                new Stroke { Number = 2, Type = "horizontal", Points = new List<Services.Point> { new Services.Point { X = 30, Y = 50 }, new Services.Point { X = 70, Y = 50 } }, Direction = "left-to-right" },
                new Stroke { Number = 3, Type = "vertical", Points = new List<Services.Point> { new Services.Point { X = 50, Y = 20 }, new Services.Point { X = 50, Y = 80 } }, Direction = "top-to-bottom" },
                new Stroke { Number = 4, Type = "hook", Points = new List<Services.Point> { new Services.Point { X = 50, Y = 80 }, new Services.Point { X = 60, Y = 90 } }, Direction = "top-left-to-bottom-right" },
                new Stroke { Number = 5, Type = "horizontal", Points = new List<Services.Point> { new Services.Point { X = 20, Y = 100 }, new Services.Point { X = 80, Y = 100 } }, Direction = "left-to-right" }
            };

            CurrentStroke = Strokes.FirstOrDefault();
            CurrentStrokeNumber = 1;
        }

        private async Task LoadStrokeOrderDataAsync()
        {
            if (_characterService == null)
            {
                // Режим дизайн-тайма
                return;
            }

            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var strokeOrderData = await _characterService.GetStrokeOrderAsync(Character);

                if (strokeOrderData == null)
                {
                    throw new InvalidOperationException($"Данные о порядке черт для иероглифа '{Character}' не найдены.");
                }

                _strokeOrderData = strokeOrderData;

                Character = strokeOrderData.Character;
                TotalStrokes = strokeOrderData.TotalStrokes;
                StrokeRules = strokeOrderData.Rules;
                CommonMistakes = strokeOrderData.CommonMistakes;
                Strokes = strokeOrderData.Strokes ?? new List<Stroke>();

                if (Strokes.Any())
                {
                    CurrentStroke = Strokes.First();
                    CurrentStrokeNumber = 1;
                }

                ResetAnimation();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Ошибка загрузки порядка черт: {ex.Message}";
                Console.WriteLine($"StrokeOrderViewModel.LoadStrokeOrderDataAsync error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Методы управления анимацией

        private void ToggleAnimation()
        {
            if (IsAnimating)
            {
                PauseAnimation();
            }
            else
            {
                StartAnimation();
            }
        }

        private void StartAnimation()
        {
            if (Strokes == null || !Strokes.Any())
                return;

            if (CurrentStrokeNumber > TotalStrokes)
            {
                ResetAnimation();
            }

            IsAnimating = true;
            _animationTimer?.Dispose();

            _animationTimer = new Timer(OnAnimationTimerTick, null, 0, (int)(_animationInterval / AnimationSpeed));

            OnPropertyChanged(nameof(PlayButtonText));
        }

        private void PauseAnimation()
        {
            IsAnimating = false;
            _animationTimer?.Dispose();
            _animationTimer = null;

            OnPropertyChanged(nameof(PlayButtonText));
        }

        private void StopAnimation()
        {
            PauseAnimation();
            ResetAnimation();
        }

        private void Rewind()
        {
            PauseAnimation();

            if (CurrentStrokeNumber > 1)
            {
                CurrentStrokeNumber--;
                CurrentStroke = Strokes[CurrentStrokeNumber - 1];
                AnimationProgress = 0;
            }
            else
            {
                ResetAnimation();
            }

            AnimationUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void StepForward()
        {
            PauseAnimation();

            if (CurrentStrokeNumber < TotalStrokes)
            {
                CurrentStrokeNumber++;
                CurrentStroke = Strokes[CurrentStrokeNumber - 1];
                AnimationProgress = 0;
            }
            else
            {
                // Достигнут конец - сбрасываем или повторяем
                if (AutoRepeat)
                {
                    ResetAnimation();
                    StartAnimation();
                }
                else
                {
                    PauseAnimation();
                }
            }

            AnimationUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void ResetAnimation()
        {
            CurrentStrokeNumber = 1;
            if (Strokes != null && Strokes.Any())
            {
                CurrentStroke = Strokes.First();
            }
            AnimationProgress = 0;

            AnimationUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnAnimationTimerTick(object state)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!IsAnimating)
                    return;

                AnimationProgress += 0.05 * AnimationSpeed;

                if (AnimationProgress >= 1.0)
                {
                    AnimationProgress = 0;

                    if (CurrentStrokeNumber < TotalStrokes)
                    {
                        CurrentStrokeNumber++;
                        CurrentStroke = Strokes[CurrentStrokeNumber - 1];
                    }
                    else
                    {
                        // Достигнут конец
                        if (AutoRepeat)
                        {
                            ResetAnimation();
                        }
                        else
                        {
                            PauseAnimation();
                        }
                    }
                }

                AnimationUpdated?.Invoke(this, EventArgs.Empty);
            });
        }

        #endregion

        #region Методы управления режимами

        private void ChangeDisplayMode(string mode)
        {
            if (Enum.TryParse<StrokeDisplayMode>(mode, out var newMode))
            {
                DisplayMode = newMode;

                // При переключении в режим всех черт показываем все черты
                if (DisplayMode == StrokeDisplayMode.AllStrokes)
                {
                    PauseAnimation();
                }

                OnPropertyChanged(nameof(IsSequentialMode));
                OnPropertyChanged(nameof(IsAllStrokesMode));
                AnimationUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void StartPractice()
        {
            IsPracticeMode = true;
            ResetAnimation();
            _userStrokePoints.Clear();
            ValidationStatus = string.Empty;
            ValidationColor = Colors.Transparent;
            _isStrokeValidated = false;
        }

        #endregion

        #region Методы практики написания

        /// <summary>
        /// Проверяет правильность черты, нарисованной пользователем.
        /// </summary>
        public void ValidateUserStroke(List<SKPoint> userPoints)
        {
            if (!IsPracticeMode || CurrentStroke == null || _isStrokeValidated)
                return;

            try
            {
                // Преобразуем точки SkiaSharp в Services.Point
                var points = userPoints.Select(p => new Services.Point { X = (int)p.X, Y = (int)p.Y }).ToList();

                // В реальном приложении здесь была бы сложная логика проверки
                // Для примера используем простую проверку направления
                bool isValid = CheckStrokeDirection(points, CurrentStroke.Direction);

                if (isValid)
                {
                    ValidationStatus = "Правильно! ✓";
                    ValidationColor = Color.FromArgb("#10B981");
                    _isStrokeValidated = true;

                    // Автоматически переходим к следующей черте через 1 секунду
                    Task.Delay(1000).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (CurrentStrokeNumber < TotalStrokes)
                            {
                                StepForward();
                                _userStrokePoints.Clear();
                                ValidationStatus = string.Empty;
                                ValidationColor = Colors.Transparent;
                                _isStrokeValidated = false;
                            }
                            else
                            {
                                ValidationStatus = "Поздравляем! Вы завершили все черты! 🎉";
                                ValidationColor = Color.FromArgb("#3B82F6");
                            }
                        });
                    });
                }
                else
                {
                    ValidationStatus = "Попробуйте еще раз. Обратите внимание на направление.";
                    ValidationColor = Color.FromArgb("#EF4444");
                }
            }
            catch (Exception ex)
            {
                ValidationStatus = $"Ошибка проверки: {ex.Message}";
                ValidationColor = Color.FromArgb("#EF4444");
                Console.WriteLine($"StrokeOrderViewModel.ValidateUserStroke error: {ex}");
            }
        }

        private bool CheckStrokeDirection(List<Services.Point> userPoints, string expectedDirection)
        {
            if (userPoints.Count < 2)
                return false;

            // Простая проверка направления по первым и последним точкам
            var firstPoint = userPoints.First();
            var lastPoint = userPoints.Last();

            switch (expectedDirection.ToLower())
            {
                case "left-to-right":
                    return lastPoint.X > firstPoint.X;
                case "right-to-left":
                    return lastPoint.X < firstPoint.X;
                case "top-to-bottom":
                    return lastPoint.Y > firstPoint.Y;
                case "bottom-to-top":
                    return lastPoint.Y < firstPoint.Y;
                case "top-left-to-bottom-right":
                    return lastPoint.X > firstPoint.X && lastPoint.Y > firstPoint.Y;
                case "top-right-to-bottom-left":
                    return lastPoint.X < firstPoint.X && lastPoint.Y > firstPoint.Y;
                default:
                    return true; // Если направление неизвестно, считаем правильным
            }
        }

        #endregion

        #region Вспомогательные методы

        private void Close()
        {
            PauseAnimation();

            // В реальном приложении здесь была бы навигация назад
            // Например: await Shell.Current.GoToAsync("..");

            Console.WriteLine("StrokeOrderView закрыт");
        }

        #endregion

        #region Вспомогательные классы

        /// <summary>
        /// Режимы отображения порядка черт.
        /// </summary>
        public enum StrokeDisplayMode
        {
            /// <summary>
            /// Последовательное отображение черт.
            /// </summary>
            Sequential,

            /// <summary>
            /// Отображение всех черт одновременно.
            /// </summary>
            AllStrokes
        }

        #endregion
    }
}
