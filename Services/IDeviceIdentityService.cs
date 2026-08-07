namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Provides a stable identifier used in the beacon payload's MAC slot.
    /// The hardware MAC is not available on modern mobile OSes, so a persisted
    /// per-install identifier is used instead.
    /// </summary>
    public interface IDeviceIdentityService
    {
        string GetDeviceIdentifier();
    }
}