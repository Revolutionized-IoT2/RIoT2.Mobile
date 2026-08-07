namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Cross-platform beacon lifecycle logic. Owns the periodic timer that
    /// rebuilds and re-advertises the encrypted payload every interval, and
    /// delegates the platform advertising to derived classes.
    /// </summary>
    public abstract class BeaconServiceBase : IBeaconService
    {
        private readonly ISettingsService _settings;
        private readonly IBeaconPayloadFactory _payloadFactory;

        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;
        private Task? _loop;

        protected BeaconServiceBase(
            ISettingsService settings,
            IBeaconPayloadFactory payloadFactory)
        {
            _settings = settings;
            _payloadFactory = payloadFactory;
        }

        public bool IsAdvertising { get; private set; }

        public async Task StartAsync()
        {
            if (IsAdvertising)
                return;

            if (!await EnsurePermissionsAsync())
                return;

            await OnStartAdvertisingAsync();
            IsAdvertising = true;

            int seconds = Math.Max(1, _settings.BeaconIntervalSeconds);
            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds));
            _loop = RunAdvertiseLoopAsync(_cts.Token);
        }

        public async Task StopAsync()
        {
            if (!IsAdvertising)
                return;

            _cts?.Cancel();
            _timer?.Dispose();

            if (_loop is not null)
            {
                try { await _loop; }
                catch (OperationCanceledException) { /* expected on stop */ }
            }

            _timer = null;
            _cts?.Dispose();
            _cts = null;
            _loop = null;

            await OnStopAdvertisingAsync();
            IsAdvertising = false;
        }

        public async Task RestartAsync()
        {
            await StopAsync();
            if (_settings.BeaconEnabled)
                await StartAsync();
        }

        private async Task RunAdvertiseLoopAsync(CancellationToken token)
        {
            // Broadcast immediately, then on every interval tick.
            await AdvertiseOnceAsync();

            while (_timer is not null && await _timer.WaitForNextTickAsync(token))
            {
                if (!_settings.BeaconEnabled)
                    break;

                await AdvertiseOnceAsync();
            }
        }

        private async Task AdvertiseOnceAsync()
        {
            byte[] data = _payloadFactory.BuildManufacturerData();
            await OnAdvertiseAsync(data);
        }

        /// <summary>Requests any platform BLE permissions. Returns false if denied.</summary>
        protected abstract Task<bool> EnsurePermissionsAsync();

        /// <summary>Prepares the platform advertiser before the first broadcast.</summary>
        protected abstract Task OnStartAdvertisingAsync();

        /// <summary>Pushes the given manufacturer data to the platform advertiser.</summary>
        protected abstract Task OnAdvertiseAsync(byte[] manufacturerData);

        /// <summary>Tears down the platform advertiser.</summary>
        protected abstract Task OnStopAdvertisingAsync();
    }
}