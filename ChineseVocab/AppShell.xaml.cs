namespace ChineseVocab
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Регистрация маршрутов для навигации
            Routing.RegisterRoute("characterDetail", typeof(Views.CharacterDetailPage));
            Routing.RegisterRoute("strokeOrder", typeof(Views.StrokeOrderPage));
        }
    }
}
