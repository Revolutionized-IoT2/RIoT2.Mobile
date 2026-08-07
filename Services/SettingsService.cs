using Microsoft.Maui.Storage;

namespace RIoT2.Mobile.Services
{
    /// <inheritdoc cref="ISettingsService" />
    public class SettingsService : ISettingsService
    {
        // Preference keys preserved from the legacy RIoT2.Android app.
        private const string DashboardUrlKey = "textCtrlUrl";
        private const string AlertsKey = "cbAlerts";
        private const string NotificationsKey = "cbNotifications";

        // BLE beacon preference keys.
        private const string BeaconEnabledKey = "beaconEnabled";
        private const string BeaconKeyKey = "beaconKey";
        private const string BeaconMessageKey = "beaconMessage";
        private const string BeaconIntervalKey = "beaconIntervalSeconds";

        private const string DefaultDashboardUrl = "http://192.168.0.108:8080/dashboard/fullscreen";
        private const int DefaultBeaconIntervalSeconds = 5;

        public string DashboardUrl
        {
            get => Preferences.Get(DashboardUrlKey, DefaultDashboardUrl);
            set => Preferences.Set(DashboardUrlKey, value);
        }

        public bool AlertsEnabled
        {
            get => Preferences.Get(AlertsKey, true);
            set => Preferences.Set(AlertsKey, value);
        }

        public bool NotificationsEnabled
        {
            get => Preferences.Get(NotificationsKey, true);
            set => Preferences.Set(NotificationsKey, value);
        }

        public bool BeaconEnabled
        {
            get => Preferences.Get(BeaconEnabledKey, false);
            set => Preferences.Set(BeaconEnabledKey, value);
        }

        public string BeaconKey
        {
            get => Preferences.Get(BeaconKeyKey, string.Empty);
            set => Preferences.Set(BeaconKeyKey, value);
        }

        public string BeaconMessage
        {
            get => Preferences.Get(BeaconMessageKey, string.Empty);
            set => Preferences.Set(BeaconMessageKey, value);
        }

        public int BeaconIntervalSeconds
        {
            get => Preferences.Get(BeaconIntervalKey, DefaultBeaconIntervalSeconds);
            set => Preferences.Set(BeaconIntervalKey, value);
        }
    }
}