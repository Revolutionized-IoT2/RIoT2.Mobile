namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Builds the encrypted manufacturer-data payload broadcast by the beacon.
    /// </summary>
    public interface IBeaconPayloadFactory
    {
        /// <summary>
        /// Builds and encrypts the payload:
        /// {unixepoch timestamp}|{mac address}|{message}.
        /// </summary>
        byte[] BuildManufacturerData();
    }
}