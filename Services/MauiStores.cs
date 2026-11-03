using Microsoft.Maui.Storage;

namespace RIoT2.Mobile.Services
{
    /// <inheritdoc cref="ISecretStore" />
    public sealed class MauiSecretStore : ISecretStore
    {
        public Task<string?> GetAsync(string key) => SecureStorage.Default.GetAsync(key);
        public Task SetAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);
        public bool Remove(string key) => SecureStorage.Default.Remove(key);
    }

    /// <inheritdoc cref="IPreferenceStore" />
    public sealed class MauiPreferenceStore : IPreferenceStore
    {
        public bool ContainsKey(string key) => Preferences.Default.ContainsKey(key);
        public string? GetString(string key) => Preferences.Default.Get<string?>(key, null);
        public void Remove(string key) => Preferences.Default.Remove(key);
    }
}
