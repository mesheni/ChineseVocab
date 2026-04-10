using ChineseVocab.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ChineseVocab
{
    public partial class StudyPage : ContentPage
    {
        private StudyViewModel? _viewModel;
        private Frame _cardFrame;
        private bool _isFlipped = false;
        private bool _isAnimating = false;
        private bool _isInitialized = false;

        public StudyPage()
        {
            InitializeComponent();
            _cardFrame = this.FindByName<Frame>("CardFrame") ?? throw new InvalidOperationException("CardFrame not found in XAML");
            BindingContextChanged += OnBindingContextChanged;
        }

        private void OnBindingContextChanged(object? sender, System.EventArgs e)
        {
            // Отписываемся от старого ViewModel
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            // Подписываемся на новый ViewModel
            _viewModel = BindingContext as StudyViewModel;
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                _isInitialized = true;
            }
        }

        private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StudyViewModel.IsAnswerVisible))
            {
                // Запускаем анимацию только если ответ стал видимым (переход с false на true)
                // и мы не в процессе анимации, и это не начальная инициализация
                if (_viewModel != null && _viewModel.IsAnswerVisible && !_isAnimating && _isInitialized)
                {
                    await FlipCardAsync(true);
                }
                // Если ответ скрывается (например, при переходе к следующей карточке),
                // сбрасываем состояние переворота
                else if (_viewModel != null && !_viewModel.IsAnswerVisible && !_isAnimating)
                {
                    // Мгновенно сбрасываем анимацию без анимации
                    _cardFrame.RotationY = 0;
                    _cardFrame.Scale = 1;
                    _isFlipped = false;
                }
            }
        }

        /// <summary>
        /// Выполняет анимацию переворота карточки.
        /// </summary>
        /// <param name="showAnswer">Если true, показывает ответ после переворота, иначе скрывает.</param>
        private async Task FlipCardAsync(bool showAnswer)
        {
            if (_isAnimating)
                return;

            _isAnimating = true;

            try
            {
                uint animationDuration = 300; // миллисекунд

                // Первая половина анимации: поворот на 90 градусов и уменьшение масштаба
                await Task.WhenAll(
                    _cardFrame.RotateYToAsync(90, animationDuration / 2, Easing.CubicInOut),
                    _cardFrame.ScaleToAsync(0.8, animationDuration / 2, Easing.CubicInOut)
                );

                // В этот момент карточка "невидима" (повернута на 90 градусов)
                // Мы можем изменить содержимое, но в нашем случае содержимое управляется ViewModel
                // и уже изменено (IsAnswerVisible = true)

                // Вторая половина анимации: поворот на 180 градусов и восстановление масштаба
                await Task.WhenAll(
                    _cardFrame.RotateYToAsync(180, animationDuration / 2, Easing.CubicInOut),
                    _cardFrame.ScaleToAsync(1.0, animationDuration / 2, Easing.CubicInOut)
                );

                _isFlipped = true;
            }
            finally
            {
                _isAnimating = false;
            }
        }

        /// <summary>
        /// Сбрасывает анимацию переворота (например, при переходе к следующей карточке).
        /// </summary>
        private void ResetCardFlip()
        {
            if (_isAnimating)
            {
                _cardFrame.AbortAnimation("RotationY");
                _cardFrame.AbortAnimation("Scale");
            }

            _cardFrame.RotationY = 0;
            _cardFrame.Scale = 1;
            _isFlipped = false;
            _isAnimating = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is StudyViewModel viewModel)
            {
                await viewModel.InitializeAsync();
                // Сбрасываем анимацию при появлении страницы
                ResetCardFlip();
            }
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            // Отписываемся от событий при скрытии страницы
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel = null;
            }

            BindingContextChanged -= OnBindingContextChanged;

            if (BindingContext is StudyViewModel viewModel)
            {
                await viewModel.OnDisappearingAsync();
            }

            // Очищаем анимации
            ResetCardFlip();
        }


    }
}
