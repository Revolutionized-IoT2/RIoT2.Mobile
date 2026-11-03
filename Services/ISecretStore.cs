namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Encrypted key-value storage for secrets. The app uses <c>SecureStorage</c>
    /// (Android Keystore, iOS Keychain) through <see cref="MauiSecretStore"/>.
    /// </summary>
    public interface ISecretStore
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value);
        bool Remove(string key);
    }

    /// <summary>
    /// Plain (unencrypted) key-value storage. The app uses <c>Preferences</c>
    /// through <see cref="MauiPreferenceStore"/>.
    /// </summary>
    public interface IPreferenceStore
    {
        bool ContainsKey(string key);
        string? GetString(string key);
        void Remove(string key);
    }
}
