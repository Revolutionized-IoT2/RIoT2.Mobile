using System.Threading.Tasks;

namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Manages Firebase Cloud Messaging: initialization, token handling, and
    /// topic subscriptions, mirroring the legacy FirebaseService behavior.
    /// </summary>
    public interface IPushNotificationService
    {
        /// <summary>
        /// Requests notification permission, retrieves the FCM token, wires up
        /// message handlers, and applies the current topic subscriptions.
        /// Safe to call multiple times; initialization runs only once.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Subscribes/unsubscribes to the "alerts" and "notifications" topics
        /// based on the current user settings.
        /// </summary>
        Task UpdateChannelSubscriptionsAsync();

        /// <summary>
        /// Handles a notification the user tapped that arrived before the FCM
        /// handlers were wired up (e.g., a cold start launched by the tap).
        /// </summary>
        Task ProcessPendingNotificationAsync();
    }
}