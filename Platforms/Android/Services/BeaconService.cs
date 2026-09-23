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
            if (_advertiser is null)
                throw new InvalidOperationException("Bluetooth advertising is unavailable. Enable Bluetooth and check device support.");

            // Keep the process alive while advertising in the background.
            BeaconForegroundService.Start();

            return Task.CompletedTask;
        }

        protected override void ValidatePayload(byte[] manufacturerData)
            => LegacyBeaconPayload.Validate(manufacturerData);

        protected override async Task OnAdvertiseAsync(byte[] manufacturerData)
        {
            if (_advertiser is null)
                throw new InvalidOperationException("Bluetooth advertiser is not initialized.");

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
            if (_callback is not null)
                _advertiser.StopAdvertising(_callback);
            var callback = new BeaconAdvertiseCallback();
            _callback = callback;
            _advertiser.StartAdvertising(settings, data, _callback);
            await callback.Completion.WaitAsync(TimeSpan.FromSeconds(10));
        }

        protected override Task OnStopAdvertisingAsync()
        {
            try
            {
                if (_advertiser is not null && _callback is not null)
                    _advertiser.StopAdvertising(_callback);
            }
            finally
            {
                _advertiser = null;
                _callback = null;
                BeaconForegroundService.Stop();
            }

            return Task.CompletedTask;
        }

        private sealed class BeaconAdvertiseCallback : AdvertiseCallback
        {
            private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public Task Completion => _completion.Task;

            public override void OnStartSuccess(AdvertiseSettings? settingsInEffect)
                => _completion.TrySetResult();

            public override void OnStartFailure(AdvertiseFailure errorCode)
                => _completion.TrySetException(new InvalidOperationException($"Bluetooth advertising failed: {errorCode}."));
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