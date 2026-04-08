using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChineseVocab.ViewModels
{
    /// <summary>
    /// Базовый класс для всех ViewModel в приложении.
    /// Предоставляет базовую функциональность для уведомления об изменениях свойств и команд.
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>
        /// Обратное свойство для IsBusy - удобно для привязки в UI.
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        /// <summary>
        /// Базовая команда для навигации назад.
        /// </summary>
        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>
        /// Базовая команда для открытия настроек.
        /// </summary>
        [RelayCommand]
        public async Task OpenSettingsAsync()
        {
            await Shell.Current.DisplayAlertAsync("Настройки", "Настройки будут доступны в следующей версии.", "OK");
        }

        /// <summary>
        /// Метод для инициализации ViewModel (можно переопределить в наследниках).
        /// </summary>
        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Метод для очистки ресурсов ViewModel (можно переопределить в наследниках).
        /// </summary>
        public virtual Task OnDisappearingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
