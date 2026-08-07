using Microsoft.Maui.Storage;

namespace RIoT2.Mobile.Services
{
    /// <inheritdoc cref="IDeviceIdentityService" />
    public class DeviceIdentityService : IDeviceIdentityService
    {
        private const string DeviceIdKey = "beaconDeviceId";

        public string GetDeviceIdentifier()
        {
            string id = Preferences.Get(DeviceIdKey, string.Empty);
            if (string.IsNullOrEmpty(id))
            {
                // Format as a MAC-like 6-byte hex string for downstream compatibility.
                byte[] bytes = Guid.NewGuid().ToByteArray();
                id = string.Join(":", bytes[..6].Select(b => b.ToString("X2")));
                Preferences.Set(DeviceIdKey, id);
            }

            return id;
        }
    }
}