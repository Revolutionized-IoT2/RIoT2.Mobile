namespace RIoT2.Mobile.Services
{
    public sealed class UnsupportedBeaconService : IBeaconService
    {
        public bool IsSupported => false;
        public string UnavailableReason => "BLE beacon advertising is not supported on this platform.";
        public bool IsAdvertising => false;
        public string? LastError => null;

        public Task StartAsync() => Task.FromException(new PlatformNotSupportedException(UnavailableReason));

        public Task RestartAsync() => StartAsync();

        public Task StopAsync() => Task.CompletedTask;
    }
}
