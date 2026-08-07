using CoreBluetooth;
using Foundation;
using RIoT2.Mobile.Services;

namespace RIoT2.Mobile.Platforms.iOS.Services
{
    /// <inheritdoc cref="BeaconServiceBase" />
    public class BeaconService : BeaconServiceBase
    {
        // iOS restricts advertising custom manufacturer data; a service UUID is
        // used and the encrypted payload is exposed via the local name field.
        private static readonly CBUUID ServiceUuid =
            CBUUID.FromString("0000FEAA-0000-1000-8000-00805F9B34FB");

        private CBPeripheralManager? _peripheralManager;

        public BeaconService(ISettingsService settings, IBeaconPayloadFactory payloadFactory)
            : base(settings, payloadFactory)
        {
        }

        protected override Task<bool> EnsurePermissionsAsync()
        {
            // Bluetooth permission is prompted on first CBPeripheralManager use
            // via NSBluetoothAlwaysUsageDescription in Info.plist.
            return Task.FromResult(true);
        }

        protected override Task OnStartAdvertisingAsync()
        {
            _peripheralManager ??= new CBPeripheralManager();
            return Task.CompletedTask;
        }

        protected override Task OnAdvertiseAsync(byte[] manufacturerData)
        {
            if (_peripheralManager is null)
                return Task.CompletedTask;

            if (_peripheralManager.Advertising)
                _peripheralManager.StopAdvertising();

            var options = new StartAdvertisingOptions
            {
                ServicesUUID = [ServiceUuid],
                LocalName = Convert.ToBase64String(manufacturerData),
            };

            _peripheralManager.StartAdvertising(options);
            return Task.CompletedTask;
        }

        protected override Task OnStopAdvertisingAsync()
        {
            if (_peripheralManager?.Advertising == true)
                _peripheralManager.StopAdvertising();

            return Task.CompletedTask;
        }
    }
}