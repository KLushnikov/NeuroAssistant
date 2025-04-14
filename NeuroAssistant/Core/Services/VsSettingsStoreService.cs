using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace NeuroAssistant.Core.Services
{
    /// <summary>
    /// Service for interacting with Visual Studio settings store.
    /// </summary>
    public interface IVsSettingsStoreService
    {
        /// <summary>
        /// Retrieves a string value from the settings store or returns a default value
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="defaultValue">Fallback value if the property is missing</param>
        /// <param name="value">Retrieved value or defaultValue</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="propertyName"/> is null, empty, or whitespace
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int GetStringOrDefault(string propertyName, string defaultValue, out string value);

        /// <summary>
        /// Saves a string value to the settings store
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Value to save</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="propertyName"/> is null, empty, or whitespace
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int SetString(string propertyName, string value);

        /// <summary>
        /// Deletes a property from the settings store
        /// </summary>
        /// <param name="propertyName">Name of the property to delete</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="propertyName"/> is null, empty, or whitespace
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int DeleteProperty(string propertyName);

        /// <summary>
        /// Deletes a property by tag and key from the settings store
        /// </summary>
        /// <param name="tag">Tag prefix</param>
        /// <param name="key">Unique key suffix</param>        
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tag"/> or <paramref name="key"/> is null, empty, or whitespace
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int DeleteProperty(string tag, string key);

        /// <summary>
        /// Retrieves a setting value by tag and key.
        /// </summary>
        /// <param name="tag">Tag prefix</param>
        /// <param name="key">Unique key suffix</param>
        /// <param name="value">Retrieved value or empty string</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tag"/> or <paramref name="key"/> is null, empty, or whitespace
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int GetTagValue(string tag, string key, out string value);

        /// <summary>
        /// Retrieves and decrypts a setting value by tag and key
        /// </summary>
        /// <param name="tag">Tag prefix</param>
        /// <param name="key">Unique key suffix</param>
        /// <param name="value">Decrypted value or empty string</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tag"/> or <paramref name="key"/> is null, empty, or whitespace
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int GetTagDecryptValue(string tag, string key, out string value);

        /// <summary>
        /// Sets a value for the specified tag and key.
        /// </summary>
        /// <param name="tag">Tag prefix</param>
        /// <param name="key">Unique key suffix</param>
        /// <param name="value">Value to store</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tag"/> or <paramref name="key"/> is null, empty, or whitespace
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int SetTagValue(string tag, string key, string value);

        /// <summary>
        /// Encrypts and sets a value for the specified tag and key
        /// </summary>
        /// <param name="tag">Tag prefix</param>
        /// <param name="key">Unique key suffix</param>
        /// <param name="value">Value to encrypt and store</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tag"/> or <paramref name="key"/> is null, empty, or whitespace
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is null
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the encryption or settings store operation fails.
        /// </exception>
        /// <returns>HRESULT status code (e.g., VSConstants.S_OK on success)</returns>
        int SetTagEncryptValue(string tag, string key, string value);
    }

    internal class VsSettingsStoreService : IVsSettingsStoreService
    {
        private const string _collectionGuid = "D7AB2C64-CF29-4F94-BED0-61E29324187A";

        private readonly IEncryptionService _encryptionService;
        private readonly IVsWritableSettingsStore _writableStore;

        /// <summary>
        /// Initializes settings store and ensures configuration collection exists.
        /// Creates the collection if it doesn't exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if settings store initialization fails or collection creation fails
        /// </exception>
        public VsSettingsStoreService(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));

            // Initializing Visual Studio settings store
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsSettingsManager settingsManager = Package.GetGlobalService(typeof(SVsSettingsManager)) as IVsSettingsManager
                ?? throw new InvalidOperationException("Не удалось получить IVsSettingsManager");

            var hResult = settingsManager.GetWritableSettingsStore(
                (uint)__VsSettingsScope.SettingsScope_UserSettings,
                out IVsWritableSettingsStore writableStore);

            if (hResult != VSConstants.S_OK || writableStore == null)
            {
                throw new COMException($"Failed to initialize the writable settings store. HRESULT: {hResult}");
            }

            _writableStore = writableStore;

            hResult = _writableStore.CollectionExists(_collectionGuid, out int existe);
            if (hResult == VSConstants.S_OK && existe == 0)
            {
                hResult = _writableStore.CreateCollection(_collectionGuid);
                if (hResult != VSConstants.S_OK)
                {
                    throw new InvalidOperationException($"Failed to create collection. HRESULT: {hResult}");
                }
            }
        }

        /// <summary>
        /// Validates that a string argument is not null, empty, or whitespace
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The name of the parameter being validated</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid</exception>
        private void ValidateStringArgument(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Parameter {paramName} cannot be not null, empty, or whitespace.", paramName);
            }
        }

        /// <summary>
        /// Combines tag and key into a composite property name using "{tag}_{key}" format.
        /// </summary>
        private string GetCompositePropertyName(string tag, string key) => $"{tag}_{key}";

        public int GetStringOrDefault(string propertyName, string defaultValue, out string value)
        {
            ValidateStringArgument(propertyName, nameof(propertyName));
            return _writableStore.GetStringOrDefault(_collectionGuid, propertyName, defaultValue, out value);
        }

        public int SetString(string propertyName, string value)
        {
            ValidateStringArgument(propertyName, nameof(propertyName));
            return _writableStore.SetString(_collectionGuid, propertyName, value ?? string.Empty);
        }

        public int DeleteProperty(string propertyName)
        {
            ValidateStringArgument(propertyName, nameof(propertyName));
            return _writableStore.DeleteProperty(_collectionGuid, propertyName);
        }

        public int DeleteProperty(string tag, string key)
        {
            ValidateStringArgument(tag, nameof(tag));
            ValidateStringArgument(key, nameof(key));

            string propertyName = GetCompositePropertyName(tag, key);
            return DeleteProperty(propertyName);
        }

        public int GetTagValue(string tag, string key, out string value)
        {
            ValidateStringArgument(tag, nameof(tag));
            ValidateStringArgument(key, nameof(key));

            string propertyName = GetCompositePropertyName(tag, key);
            return GetStringOrDefault(propertyName, string.Empty, out value);
        }

        public int GetTagDecryptValue(string tag, string key, out string value)
        {
            var result = GetTagValue(tag, key, out value);
            value = result == VSConstants.S_OK ? _encryptionService.Decrypt(value) : string.Empty;
            return result;
        }

        public int SetTagValue(string tag, string key, string value)
        {
            ValidateStringArgument(tag, nameof(tag));
            ValidateStringArgument(key, nameof(key));

            string propertyName = GetCompositePropertyName(tag, key);
            return SetString(propertyName, value ?? string.Empty);
        }

        public int SetTagEncryptValue(string tag, string key, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string encryptedValue = _encryptionService.Encrypt(value);
            return SetTagValue(tag, key, encryptedValue);
        }
    }
}
