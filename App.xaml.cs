using RIoT2.Mobile.Services;

namespace RIoT2.Mobile
{
    public partial class App : Application
    {
        private readonly IPushNotificationService _pushNotifications;

        public App(IPushNotificationService pushNotifications)
        {
            InitializeComponent();
            _pushNotifications = pushNotifications;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Fire-and-forget initialization; failures are handled/logged internally.
            _ = _pushNotifications.InitializeAsync();

            return new Window(new AppShell());
        }
    }
}