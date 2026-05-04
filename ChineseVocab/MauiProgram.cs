using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using ChineseVocab.ViewModels;
using ChineseVocab.Services;
using ChineseVocab.Services.Audio;
using ChineseVocab.Services.Character;

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
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ISchedulerService, SchedulerService>();

            // Регистрация сервиса аудио-озвучки (TextToSpeech)
            services.AddSingleton<ITextToSpeechService, TextToSpeechService>();

            // Регистрация загрузчика данных порядка черт
            services.AddSingleton<StrokeOrderDataLoader>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            // Регистрация ViewModels как Transient (создается новый экземпляр для каждой навигации)
            services.AddTransient<MainViewModel>();
            services.AddTransient<StudyViewModel>();
            services.AddTransient<DictationViewModel>();

            services.AddTransient<CharacterLibraryViewModel>();
            // TODO: Добавить остальные ViewModels по мере создания
            // services.AddTransient<SentencesViewModel>();
            // services.AddTransient<StatisticsViewModel>();
        }
    }
}
