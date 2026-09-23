using RIoT2.Mobile.Services;
using Microsoft.Extensions.Logging;
using Application = Microsoft.Maui.Controls.Application;

namespace RIoT2.Mobile
{
    public partial class App : Application
    {
        private readonly IPushNotificationService _pushNotifications;
        private readonly ISettingsService _settings;
        private readonly IBeaconService _beacon;
        private readonly ILogger<App> _logger;

        public App(
            IPushNotificationService pushNotifications,
            ISettingsService settings,
            IBeaconService beacon,
            ILogger<App> logger)
        {
            InitializeComponent();
            _pushNotifications = pushNotifications;
            _settings = settings;
            _beacon = beacon;
            _logger = logger;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Fire-and-forget initialization; failures are handled/logged internally.
            _ = _pushNotifications.InitializeAsync();

            // Resume the beacon if it was enabled on a previous run.
            if (_settings.BeaconEnabled)
                _ = ResumeBeaconAsync();

            return new Window(new AppShell());
        }

        private async Task ResumeBeaconAsync()
        {
            if (!_beacon.IsSupported)
            {
                _logger.LogWarning("{Reason}", _beacon.UnavailableReason);
                return;
            }

            try { await _beacon.StartAsync(); }
            catch (Exception ex) { _logger.LogError(ex, "Could not resume BLE beacon advertising."); }
        }
    }
}