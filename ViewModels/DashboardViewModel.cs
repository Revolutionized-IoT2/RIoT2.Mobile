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
        private string _settingsUrl;
        private bool _active;

        public event EventHandler? RefreshRequested;

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
            _settingsUrl = _source;

            _isConnected = _connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        /// <summary>Reloads the dashboard using the latest saved URL.</summary>
        [RelayCommand]
        private void Refresh()
        {
            _settingsUrl = _settings.DashboardUrl;
            if (Source != _settingsUrl)
                Source = _settingsUrl;
            else
                RequestRefresh();
        }

        partial void OnSourceChanged(string value)
        {
            if (_active)
                RequestRefresh();
        }

        private void RequestRefresh()
        {
            if (!_active || RefreshRequested is null)
            {
                CompleteNavigation();
                return;
            }

            IsLoading = true;
            IsRefreshing = true;
            try { RefreshRequested.Invoke(this, EventArgs.Empty); }
            catch
            {
                CompleteNavigation();
                throw;
            }
        }

        public void CompleteNavigation()
        {
            IsLoading = false;
            IsRefreshing = false;
        }

        public void Activate()
        {
            if (_active)
                return;

            if (_settingsUrl != _settings.DashboardUrl)
            {
                _settingsUrl = _settings.DashboardUrl;
                Source = _settingsUrl;
            }
            IsConnected = _connectivity.NetworkAccess == NetworkAccess.Internet;
            _connectivity.ConnectivityChanged += OnConnectivityChanged;
            _active = true;
            RequestRefresh();
        }

        public void Deactivate()
        {
            _active = false;
            _connectivity.ConnectivityChanged -= OnConnectivityChanged;
            CompleteNavigation();
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!_active)
                    return;
                var wasConnected = IsConnected;
                IsConnected = e.NetworkAccess == NetworkAccess.Internet;
                if (!wasConnected && IsConnected)
                    RequestRefresh();
                else if (!IsConnected)
                    CompleteNavigation();
            });
        }

        public void Dispose()
        {
            Deactivate();
        }
    }
}