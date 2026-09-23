namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Controls the BLE beacon advertising lifecycle.
    /// </summary>
    public interface IBeaconService
    {
        bool IsSupported { get; }

        string? UnavailableReason { get; }

        bool IsAdvertising { get; }

        string? LastError { get; }

        Task StartAsync();

        Task StopAsync();

        /// <summary>Applies the latest settings by stopping and restarting.</summary>
        Task RestartAsync();
    }
}