using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RIoT2.Mobile.Services;
using RIoT2.Mobile.Views;

namespace RIoT2.Mobile.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ISettingsService _settings;

        [ObservableProperty]
        private string _source;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRefreshing;

        public DashboardViewModel(ISettingsService settings)
        {
            _settings = settings;
            _source = _settings.DashboardUrl;
        }

        /// <summary>Reloads the dashboard using the latest saved URL.</summary>
        [RelayCommand]
        private void Refresh()
        {
            // Reassigning Source forces the WebView to reload the (possibly updated) URL.
            Source = _settings.DashboardUrl;
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}