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
        private readonly SemaphoreSlim _lifecycle = new(1, 1);

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

        public bool IsSupported => true;
        public string? UnavailableReason => null;
        public bool IsAdvertising { get; private set; }
        public string? LastError { get; private set; }

        public async Task StartAsync()
        {
            await _lifecycle.WaitAsync();
            try { await StartCoreAsync(); }
            finally { _lifecycle.Release(); }
        }

        private async Task StartCoreAsync()
        {
            if (IsAdvertising)
                return;

            await StopCoreAsync();
            LastError = null;
            try
            {
                byte[] data = _payloadFactory.BuildManufacturerData();
                ValidatePayload(data);
                if (!await EnsurePermissionsAsync())
                    throw new InvalidOperationException("Bluetooth advertising permission was denied.");

                await OnStartAdvertisingAsync();
                await OnAdvertiseAsync(data);
                IsAdvertising = true;
                int seconds = Math.Max(1, _settings.BeaconIntervalSeconds);
                _cts = new CancellationTokenSource();
                _timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds));
                _loop = RunAdvertiseLoopAsync(_timer, _cts.Token);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                IsAdvertising = false;
                try { await OnStopAdvertisingAsync(); }
                catch { /* Preserve the original startup failure. */ }
                throw;
            }
        }

        public async Task StopAsync()
        {
            await _lifecycle.WaitAsync();
            try { await StopCoreAsync(); }
            finally { _lifecycle.Release(); }
        }

        private async Task StopCoreAsync()
        {
            _cts?.Cancel();

            if (_loop is not null)
                await _loop;

            _timer?.Dispose();
            _timer = null;
            _cts?.Dispose();
            _cts = null;
            _loop = null;

            IsAdvertising = false;
        }

        public async Task RestartAsync()
        {
            await _lifecycle.WaitAsync();
            try
            {
                await StopCoreAsync();
                if (_settings.BeaconEnabled)
                    await StartCoreAsync();
            }
            finally { _lifecycle.Release(); }
        }

        private async Task RunAdvertiseLoopAsync(PeriodicTimer timer, CancellationToken token)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    if (!_settings.BeaconEnabled)
                        break;
                    await AdvertiseOnceAsync();
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
            finally
            {
                IsAdvertising = false;
                timer.Dispose();
                try { await OnStopAdvertisingAsync(); }
                catch (Exception ex) { LastError ??= ex.Message; }
            }
        }

        private async Task AdvertiseOnceAsync()
        {
            byte[] data = _payloadFactory.BuildManufacturerData();
            ValidatePayload(data);
            await OnAdvertiseAsync(data);
        }

        protected virtual void ValidatePayload(byte[] manufacturerData) { }

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