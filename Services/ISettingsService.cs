namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Persists user settings. Keys are kept identical to the legacy
    /// RIoT2.Android app so existing user data is preserved on upgrade.
    /// The beacon key is a secret and lives in <see cref="IBeaconKeyStore"/>.
    /// </summary>
    public interface ISettingsService
    {
        string DashboardUrl { get; set; }
        bool AlertsEnabled { get; set; }
        bool NotificationsEnabled { get; set; }

        // BLE beacon
        bool BeaconEnabled { get; set; }
        string BeaconMessage { get; set; }
        int BeaconIntervalSeconds { get; set; }
    }
}