namespace RIoT2.Mobile.Services
{
    /// <inheritdoc cref="IBeaconPayloadFactory" />
    public class BeaconPayloadFactory : IBeaconPayloadFactory
    {
        private readonly ISettingsService _settings;
        private readonly IDeviceIdentityService _identity;
        private readonly ICryptoService _crypto;

        public BeaconPayloadFactory(
            ISettingsService settings,
            IDeviceIdentityService identity,
            ICryptoService crypto)
        {
            _settings = settings;
            _identity = identity;
            _crypto = crypto;
        }

        public byte[] BuildManufacturerData()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string mac = _identity.GetDeviceIdentifier();
            string message = _settings.BeaconMessage ?? string.Empty;

            string payload = $"{timestamp}|{mac}|{message}";

            return _crypto.Encrypt(payload, _settings.BeaconKey);
        }
    }
}