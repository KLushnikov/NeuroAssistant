using System;
using System.Security.Cryptography;
using System.Text;

namespace NeuroAssistant.Core.Services
{
    /// <summary>
    /// Provides encryption/decryption functionality
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a plain text string
        /// </summary>
        /// <param name="input">Plain text to encrypt</param>
        /// <returns>Base64-encoded encrypted data</returns>
        string Encrypt(string input);
        /// <summary>
        /// Decrypts data encrypted
        /// </summary>
        /// <param name="encrypted">Base64-encoded encrypted data</param>
        /// <returns>Decrypted plain text</returns>
        string Decrypt(string encrypted);
    }

    /// <summary>
    /// DPAPI-based implementation of IEncryptionService
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        /// <summary>
        /// Encrypts data using CurrentUser scope without additional entropy
        /// </summary>
        public string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var encryptedData = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(input),
                optionalEntropy: null,// Optional additional secret
                scope: DataProtectionScope.CurrentUser
            );
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts data encrypted
        /// </summary>
        /// <exception cref="CryptographicException">
        /// Thrown for invalid/malformed input or if decryption fails
        /// </exception>
        public string Decrypt(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
            {
                return string.Empty;
            }

            var decryptedData = ProtectedData.Unprotect(
                Convert.FromBase64String(encrypted),
                optionalEntropy: null,// Must match entropy used for encryption
                scope: DataProtectionScope.CurrentUser
            );
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
