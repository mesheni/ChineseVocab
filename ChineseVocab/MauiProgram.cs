using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using ChineseVocab.ViewModels;
using ChineseVocab.Services;

namespace ChineseVocab
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMarkup()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Регистрация сервисов
            RegisterServices(builder.Services);

            // Регистрация ViewModels
            RegisterViewModels(builder.Services);

            return builder.Build();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // Регистрация сервиса базы данных
            services.AddSingleton<IDatabaseService, DatabaseService>();

            // Регистрация сервиса SRS (системы интервальных повторений)
            services.AddSingleton<ISRSService, SRSService>();

            // Регистрация сервиса для работы с иероглифами
            services.AddSingleton<ICharacterService, CharacterService>();

            // Регистрация сервиса для работы с примерами предложений
            services.AddSingleton<ISentenceService, SentenceService>();

            // Регистрация сервиса статистики
            services.AddSingleton<IStatisticsService, StatisticsService>();

            // Регистрация сервиса планирования повторений
            services.AddSingleton<ISchedulerService, SchedulerService>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            // Регистрация ViewModels как Transient (создается новый экземпляр для каждой навигации)
            services.AddTransient<MainViewModel>();
            services.AddTransient<StudyViewModel>();
            services.AddTransient<DictationViewModel>();

            // TODO: Добавить остальные ViewModels по мере создания
            // services.AddTransient<CharacterLibraryViewModel>();
            // services.AddTransient<SentencesViewModel>();
            // services.AddTransient<StatisticsViewModel>();
        }
    }
}
