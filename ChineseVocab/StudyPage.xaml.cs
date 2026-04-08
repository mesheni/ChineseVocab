using ChineseVocab.ViewModels;

namespace ChineseVocab
{
    public partial class StudyPage : ContentPage
    {
        public StudyPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is StudyViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (BindingContext is StudyViewModel viewModel)
            {
                await viewModel.OnDisappearingAsync();
            }
        }
    }
}
