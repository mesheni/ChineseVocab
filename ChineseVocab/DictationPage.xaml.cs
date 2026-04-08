using ChineseVocab.ViewModels;

namespace ChineseVocab
{
    public partial class DictationPage : ContentPage
    {
        public DictationPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is DictationViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (BindingContext is DictationViewModel viewModel)
            {
                await viewModel.OnDisappearingAsync();
            }
        }

        // Обработчик события Completed для поля ввода (вызывается при нажатии Enter)
        private void OnEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is DictationViewModel viewModel)
            {
                // Автоматически проверяем ответ при нажатии Enter
                viewModel.CheckAnswerCommand.Execute(null);
            }
        }
    }
}
