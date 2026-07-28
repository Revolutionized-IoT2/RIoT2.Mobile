using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.Firebase.CloudMessaging;

namespace RIoT2.Mobile.Services
{
    /// <inheritdoc cref="IPushNotificationService" />
    /// <remarks>
    /// Replaces the legacy FirebaseService. Topic subscription logic matches
    /// UpdateChannelSubscriptions() from RIoT2.Android.
    /// </remarks>
    public class PushNotificationService : IPushNotificationService
    {
        private const string AlertsTopic = "alerts";
        private const string NotificationsTopic = "notifications";

        private readonly ISettingsService _settings;
        private readonly ILogger<PushNotificationService> _logger;
        private bool _isInitialized;

        public PushNotificationService(ISettingsService settings, ILogger<PushNotificationService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                // Ask the user for notification permission (required on Android 13+ and iOS).
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

                // Log the token; the legacy app's SendRegistrationToServer was a no-op.
                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                _logger.LogInformation("FCM token: {Token}", token);

                CrossFirebaseCloudMessaging.Current.TokenChanged += (_, e) =>
                    _logger.LogInformation("FCM token changed: {Token}", e.Token);

                CrossFirebaseCloudMessaging.Current.NotificationReceived += (_, e) =>
                    _logger.LogInformation("FCM notification received: {Title}", e.Notification?.Title);

                await UpdateChannelSubscriptionsAsync();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize push notifications.");
            }
        }

        public async Task UpdateChannelSubscriptionsAsync()
        {
            var messaging = CrossFirebaseCloudMessaging.Current;

            try
            {
                if (_settings.AlertsEnabled)
                    await messaging.SubscribeToTopicAsync(AlertsTopic);
                else
                    await messaging.UnsubscribeFromTopicAsync(AlertsTopic);

                if (_settings.NotificationsEnabled)
                    await messaging.SubscribeToTopicAsync(NotificationsTopic);
                else
                    await messaging.UnsubscribeFromTopicAsync(NotificationsTopic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update FCM topic subscriptions.");
            }
        }
    }
}