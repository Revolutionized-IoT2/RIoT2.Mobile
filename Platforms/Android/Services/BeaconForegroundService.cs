using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace RIoT2.Mobile.Platforms.Android.Services
{
    /// <summary>
    /// Foreground service that keeps the app process alive while the BLE beacon
    /// is advertising, so the interval timer and advertiser continue to run when
    /// the app is backgrounded. It shows a persistent, low-importance notification
    /// as required by Android for foreground services.
    /// </summary>
    [Service(
        Exported = false,
        ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeConnectedDevice)]
    public class BeaconForegroundService : Service
    {
        private const string ChannelId = "riot2_beacon_channel";
        private const int NotificationId = 8021;

        public static void Start()
        {
            var context = global::Android.App.Application.Context;
            var intent = new Intent(context, typeof(BeaconForegroundService));

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
        }

        public static void Stop()
        {
            var context = global::Android.App.Application.Context;
            var intent = new Intent(context, typeof(BeaconForegroundService));
            context.StopService(intent);
        }

        public override IBinder? OnBind(Intent? intent) => null;

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            CreateNotificationChannel();

            var notification = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle("RIoT2 beacon active")
                .SetContentText("Broadcasting BLE beacon.")
                .SetSmallIcon(global::Android.Resource.Drawable.StatSysDataBluetooth)
                .SetOngoing(true)
                .SetPriority((int)NotificationPriority.Low)
                .Build();

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                StartForeground(
                    NotificationId,
                    notification,
                    global::Android.Content.PM.ForegroundService.TypeConnectedDevice);
            }
            else
            {
                StartForeground(NotificationId, notification);
            }

            // Restart the service if the system kills it while advertising.
            return StartCommandResult.Sticky;
        }

        private void CreateNotificationChannel()
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(26))
                return;

            var manager = (NotificationManager?)GetSystemService(NotificationService);
            if (manager?.GetNotificationChannel(ChannelId) is not null)
                return;

            var channel = new NotificationChannel(
                ChannelId,
                "RIoT2 Beacon",
                NotificationImportance.Low)
            {
                Description = "Keeps the BLE beacon advertising in the background.",
            };

            manager?.CreateNotificationChannel(channel);
        }
    }
}
