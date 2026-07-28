namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Persists user settings. Keys are kept identical to the legacy
    /// RIoT2.Android app so existing user data is preserved on upgrade.
    /// </summary>
    public interface ISettingsService
    {
        string DashboardUrl { get; set; }
        bool AlertsEnabled { get; set; }
        bool NotificationsEnabled { get; set; }
    }
}