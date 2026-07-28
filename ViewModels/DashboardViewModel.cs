using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RIoT2.Mobile.Services;
using RIoT2.Mobile.Views;

namespace RIoT2.Mobile.ViewModels
{
    [QueryProperty(nameof(Source), nameof(Source))]
    public partial class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly ISettingsService _settings;
        private readonly IConnectivity _connectivity;

        [ObservableProperty]
        private string _source;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _isConnected;

        public DashboardViewModel(ISettingsService settings, IConnectivity connectivity)
        {
            _settings = settings;
            _connectivity = connectivity;
            _source = _settings.DashboardUrl;

            _isConnected = _connectivity.NetworkAccess == NetworkAccess.Internet;
            _connectivity.ConnectivityChanged += OnConnectivityChanged;
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

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            var wasConnected = IsConnected;
            IsConnected = e.NetworkAccess == NetworkAccess.Internet;

            // Auto-reload the dashboard when connectivity is restored, so the
            // user doesn't have to manually refresh after losing the network.
            if (!wasConnected && IsConnected)
            {
                Refresh();
            }
        }

        public void Dispose()
        {
            _connectivity.ConnectivityChanged -= OnConnectivityChanged;
        }
    }
}