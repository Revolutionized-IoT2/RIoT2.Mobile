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

        private const string DefaultDashboardUrl = "http://192.168.0.108:8080/dashboard/fullscreen";

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
    }
}