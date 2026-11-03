namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Stores the BLE beacon AES key in encrypted storage.
    /// </summary>
    public interface IBeaconKeyStore
    {
        /// <summary>Returns the key, or an empty string when none is set.</summary>
        Task<string> GetAsync();

        /// <summary>Stores the key. An empty value deletes it.</summary>
        Task SetAsync(string value);
    }
}
