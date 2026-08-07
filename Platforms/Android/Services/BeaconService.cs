using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using RIoT2.Mobile.Services;

namespace RIoT2.Mobile.Platforms.Android.Services
{
    /// <inheritdoc cref="BeaconServiceBase" />
    public class BeaconService : BeaconServiceBase
    {
        // Manufacturer/company id used to tag the advertised data.
        // Replace 0xFFFF (reserved for testing) with a registered id if available.
        private const int ManufacturerId = 0xFFFF;

        private BluetoothLeAdvertiser? _advertiser;
        private AdvertiseCallback? _callback;

        public BeaconService(ISettingsService settings, IBeaconPayloadFactory payloadFactory)
            : base(settings, payloadFactory)
        {
        }

        protected override async Task<bool> EnsurePermissionsAsync()
        {
            // BLUETOOTH_ADVERTISE is a runtime permission from Android 12 (API 31).
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                var status = await Permissions.RequestAsync<BluetoothAdvertisePermission>();
                return status == PermissionStatus.Granted;
            }

            return true;
        }

        protected override Task OnStartAdvertisingAsync()
        {
            var manager = (BluetoothManager?)global::Android.App.Application.Context
                .GetSystemService(Context.BluetoothService);

            _advertiser = manager?.Adapter?.BluetoothLeAdvertiser;
            _callback = new BeaconAdvertiseCallback();

            // Keep the process alive while advertising in the background.
            BeaconForegroundService.Start();

            return Task.CompletedTask;
        }

        protected override Task OnAdvertiseAsync(byte[] manufacturerData)
        {
            if (_advertiser is null || _callback is null)
                return Task.CompletedTask;

            var settings = new AdvertiseSettings.Builder()
                .SetAdvertiseMode(AdvertiseMode.LowLatency)!
                .SetTxPowerLevel(AdvertiseTx.PowerHigh)!
                .SetConnectable(false)!
                .Build();

            var data = new AdvertiseData.Builder()
                .AddManufacturerData(ManufacturerId, manufacturerData)!
                .SetIncludeDeviceName(false)!
                .Build();

            // Restart advertising so the refreshed payload takes effect.
            _advertiser.StopAdvertising(_callback);
            _advertiser.StartAdvertising(settings, data, _callback);

            return Task.CompletedTask;
        }

        protected override Task OnStopAdvertisingAsync()
        {
            if (_advertiser is not null && _callback is not null)
                _advertiser.StopAdvertising(_callback);

            _advertiser = null;
            _callback = null;

            BeaconForegroundService.Stop();

            return Task.CompletedTask;
        }

        private sealed class BeaconAdvertiseCallback : AdvertiseCallback
        {
            public override void OnStartFailure(AdvertiseFailure errorCode)
                => base.OnStartFailure(errorCode);
        }
    }

    /// <summary>Runtime permission wrapper for BLUETOOTH_ADVERTISE.</summary>
    public class BluetoothAdvertisePermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        [
            (global::Android.Manifest.Permission.BluetoothAdvertise, true),
            (global::Android.Manifest.Permission.BluetoothConnect, true),
        ];
    }
}