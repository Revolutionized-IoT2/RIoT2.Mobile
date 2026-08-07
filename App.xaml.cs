using RIoT2.Mobile.Services;
using Application = Microsoft.Maui.Controls.Application;

namespace RIoT2.Mobile
{
    public partial class App : Application
    {
        private readonly IPushNotificationService _pushNotifications;
        private readonly ISettingsService _settings;
        private readonly IBeaconService _beacon;

        public App(
            IPushNotificationService pushNotifications,
            ISettingsService settings,
            IBeaconService beacon)
        {
            InitializeComponent();
            _pushNotifications = pushNotifications;
            _settings = settings;
            _beacon = beacon;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Fire-and-forget initialization; failures are handled/logged internally.
            _ = _pushNotifications.InitializeAsync();

            // Resume the beacon if it was enabled on a previous run.
            if (_settings.BeaconEnabled)
                _ = _beacon.StartAsync();

            return new Window(new AppShell());
        }
    }
}