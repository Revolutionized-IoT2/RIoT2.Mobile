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

        [ObservableProperty]
        private string _dashboardUrl;

        [ObservableProperty]
        private bool _alertsEnabled;

        [ObservableProperty]
        private bool _notificationsEnabled;

        public SettingsViewModel(ISettingsService settings, IPushNotificationService push)
        {
            _settings = settings;
            _push = push;

            _dashboardUrl = _settings.DashboardUrl;
            _alertsEnabled = _settings.AlertsEnabled;
            _notificationsEnabled = _settings.NotificationsEnabled;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            _settings.DashboardUrl = DashboardUrl;
            _settings.AlertsEnabled = AlertsEnabled;
            _settings.NotificationsEnabled = NotificationsEnabled;

            await _push.UpdateChannelSubscriptionsAsync();

            // Return to the dashboard.
            await Shell.Current.GoToAsync("..");
        }
    }
}