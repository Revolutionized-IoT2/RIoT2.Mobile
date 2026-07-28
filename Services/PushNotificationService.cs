using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Plugin.Firebase.CloudMessaging;
using RIoT2.Mobile.ViewModels;

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

        // Data-payload keys used for deep-linking. The server includes these in
        // the FCM message's "data" section, e.g. { "route": "//dashboard" }.
        private const string RouteKey = "route";
        private const string UrlKey = "url";

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

                // Deep-link when the user taps a notification. This also fires for
                // the notification that cold-started the app, once handlers are wired.
                CrossFirebaseCloudMessaging.Current.NotificationTapped += (_, e) =>
                    _ = HandleNotificationTappedAsync(e.Notification);

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

        /// <summary>
        /// Navigates based on the tapped notification's data payload. Supports a
        /// Shell <c>route</c> and/or a <c>url</c> to open in the dashboard.
        /// </summary>
        private async Task HandleNotificationTappedAsync(FCMNotification? notification)
        {
            if (notification?.Data is null)
                return;

            var data = notification.Data;
            data.TryGetValue(RouteKey, out var route);
            data.TryGetValue(UrlKey, out var url);

            if (string.IsNullOrWhiteSpace(route) && string.IsNullOrWhiteSpace(url))
                return;

            // Shell navigation must run on the UI thread.
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (Shell.Current is null)
                        return;

                    // Default to the dashboard route when only a URL was provided.
                    var target = string.IsNullOrWhiteSpace(route) ? "//dashboard" : route!;

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        await Shell.Current.GoToAsync(target, new Dictionary<string, object>
                        {
                            [nameof(DashboardViewModel.Source)] = url!
                        });
                    }
                    else
                    {
                        await Shell.Current.GoToAsync(target);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deep-link from notification.");
                }
            });
        }
    }
}