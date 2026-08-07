using System.Security.Cryptography;
using System.Text;

namespace RIoT2.Mobile.Services
{
    /// <inheritdoc cref="ICryptoService" />
    public class AesCryptoService : ICryptoService
    {
        private const int NonceSize = 12; // AES-GCM standard nonce size.
        private const int TagSize = 16;    // AES-GCM authentication tag size.

        public byte[] Encrypt(string plainText, string sharedKey)
        {
            ArgumentNullException.ThrowIfNull(plainText);

            if (string.IsNullOrEmpty(sharedKey))
                throw new ArgumentException("Shared key must be provided.", nameof(sharedKey));

            // Derive a stable 256-bit key from the shared secret.
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(sharedKey));
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[TagSize];

            using var aesGcm = new AesGcm(key, TagSize);
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

            // Layout: nonce | tag | ciphertext
            byte[] result = new byte[NonceSize + TagSize + cipherBytes.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

            return result;
        }
    }
}