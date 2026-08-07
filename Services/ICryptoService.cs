namespace RIoT2.Mobile.Services
{
    /// <summary>
    /// Symmetric encryption for beacon payloads using a shared secret key.
    /// </summary>
    public interface ICryptoService
    {
        /// <summary>
        /// Encrypts <paramref name="plainText"/> with a key derived from
        /// <paramref name="sharedKey"/>. Output layout: nonce | tag | ciphertext.
        /// </summary>
        byte[] Encrypt(string plainText, string sharedKey);
    }
}