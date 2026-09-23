namespace RIoT2.Mobile.Services
{
    public static class LegacyBeaconPayload
    {
        // The 31-byte advertisement includes length, AD type, and company ID.
        public const int MaximumManufacturerDataLength = 27;

        public static void Validate(byte[] manufacturerData)
        {
            ArgumentNullException.ThrowIfNull(manufacturerData);
            if (manufacturerData.Length > MaximumManufacturerDataLength)
                throw new InvalidOperationException(
                    $"Encrypted beacon payload is {manufacturerData.Length} bytes; legacy Bluetooth advertising " +
                    $"allows at most {MaximumManufacturerDataLength} manufacturer-data bytes. " +
                    "The current beacon protocol cannot fit; a compatible transport/protocol update is required.");
        }
    }
}
