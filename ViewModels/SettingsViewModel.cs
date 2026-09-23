using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RIoT2.Mobile.Services;

namespace RIoT2.Mobile.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settings;
        private readonly IPushNotificationService _push;
        private readonly IBeaconService _beacon;

        public bool IsBeaconSupported => _beacon.IsSupported;
        public string? BeaconUnavailableReason => _beacon.UnavailableReason;

        private string? _beaconError;

        public string? BeaconError
        {
            get => _beaconError;
            private set
            {
                if (SetProperty(ref _beaconError, value))
                    OnPropertyChanged(nameof(HasBeaconError));
            }
        }

        public bool HasBeaconError => !string.IsNullOrEmpty(BeaconError);

        [ObservableProperty]
        private string _dashboardUrl;

        [ObservableProperty]
        private bool _alertsEnabled;

        [ObservableProperty]
        private bool _notificationsEnabled;

        [ObservableProperty]
        private bool _beaconEnabled;

        [ObservableProperty]
        private string _beaconKey;

        [ObservableProperty]
        private string _beaconMessage;

        [ObservableProperty]
        private int _beaconIntervalSeconds;

        public SettingsViewModel(
            ISettingsService settings,
            IPushNotificationService push,
            IBeaconService beacon)
        {
            _settings = settings;
            _push = push;
            _beacon = beacon;

            _dashboardUrl = _settings.DashboardUrl;
            _alertsEnabled = _settings.AlertsEnabled;
            _notificationsEnabled = _settings.NotificationsEnabled;

            _beaconEnabled = IsBeaconSupported && _settings.BeaconEnabled;
            _beaconError = _beacon.LastError;
            _beaconKey = _settings.BeaconKey;
            _beaconMessage = _settings.BeaconMessage;
            _beaconIntervalSeconds = _settings.BeaconIntervalSeconds;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            _settings.DashboardUrl = DashboardUrl;
            _settings.AlertsEnabled = AlertsEnabled;
            _settings.NotificationsEnabled = NotificationsEnabled;

            _settings.BeaconEnabled = IsBeaconSupported && BeaconEnabled;
            _settings.BeaconKey = BeaconKey;
            _settings.BeaconMessage = BeaconMessage;
            _settings.BeaconIntervalSeconds = BeaconIntervalSeconds;

            await _push.UpdateChannelSubscriptionsAsync();

            BeaconError = null;
            try
            {
                if (_settings.BeaconEnabled)
                    await _beacon.RestartAsync();
                else
                    await _beacon.StopAsync();
            }
            catch (Exception ex)
            {
                BeaconError = ex.Message;
                return;
            }

            // Return to the dashboard.
            await Shell.Current.GoToAsync("..");
        }
    }
}