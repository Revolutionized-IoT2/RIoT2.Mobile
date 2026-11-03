namespace RIoT2.Mobile.Services
{
    /// <inheritdoc cref="IBeaconKeyStore" />
    /// <remarks>
    /// Versions up to 1.0.0 kept the key in plain <c>Preferences</c>. The first read moves
    /// that value into the secret store and deletes the plain copy. The key is cached in
    /// memory because the beacon rebuilds its payload every few seconds.
    /// </remarks>
    public sealed class BeaconKeyStore : IBeaconKeyStore
    {
        // Same key name as the legacy preference, so the migration finds it.
        internal const string StorageKey = "beaconKey";

        private readonly ISecretStore _secrets;
        private readonly IPreferenceStore _preferences;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private string? _cached;

        public BeaconKeyStore(ISecretStore secrets, IPreferenceStore preferences)
        {
            _secrets = secrets;
            _preferences = preferences;
        }

        public async Task<string> GetAsync()
        {
            await _gate.WaitAsync();
            try
            {
                if (_cached is not null)
                    return _cached;

                // A read error propagates and nothing is cached, so the next call retries. MAUI itself
                // deletes an entry it can't decrypt and returns null; then the user enters the key again.
                string? stored = await _secrets.GetAsync(StorageKey);

                if (_preferences.ContainsKey(StorageKey))
                {
                    string legacy = _preferences.GetString(StorageKey) ?? string.Empty;
                    if (string.IsNullOrEmpty(stored) && legacy.Length > 0)
                    {
                        await _secrets.SetAsync(StorageKey, legacy);
                        stored = legacy;
                    }
                    _preferences.Remove(StorageKey);
                }

                _cached = stored ?? string.Empty;
                return _cached;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task SetAsync(string value)
        {
            value ??= string.Empty;
            await _gate.WaitAsync();
            try
            {
                if (value.Length == 0)
                    _secrets.Remove(StorageKey);
                else
                    await _secrets.SetAsync(StorageKey, value);

                _preferences.Remove(StorageKey);
                _cached = value;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
