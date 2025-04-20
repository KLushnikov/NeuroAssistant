using Microsoft.VisualStudio;
using NeuroAssistant.Ai.Connection;
using NeuroAssistant.Core.Enum;
using NeuroAssistant.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NeuroAssistant.Ai.Profiles
{
    /// <summary>
    /// Manages AI connection profiles by storing and retrieving settings
    /// </summary>
    public interface IAiProfileManager
    {
        /// <summary>
        /// Retrieves all stored profile names from the settings store
        /// </summary>
        /// <returns>List of profile names. Returns empty list if no profiles exist</returns>
        List<string> GetProfileIds();

        /// <summary>
        /// Saves or updates a connection profile in the settings store
        /// </summary>
        /// <param name="settings">Profile settings to persist</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null</exception>
        /// <returns>
        /// <see cref="NeuroAssistantResult.AddNewItem"/> when creating a new profile, 
        /// <see cref="NeuroAssistantResult.Ok"/> when updating existing
        /// </returns>
        NeuroAssistantResult SaveProfile(IAiConnectionSettings settings);

        /// <summary>
        /// Loads a specific profile from the settings store
        /// </summary>
        /// <typeparam name="T">Type implementing <see cref="IAiConnectionSettings"/> with parameterless constructor</typeparam>
        /// <param name="profileName">Name of the profile to load</param>
        /// <exception cref="KeyNotFoundException">Thrown if profile doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown for invalid/missing data or parsing errors (wraps original exceptions)</exception>
        /// <returns>Initialized settings object</returns>
        IAiConnectionSettings LoadProfile<T>(string profileName) where T : IAiConnectionSettings, new();

        /// <summary>
        /// Removes a profile and its associated settings
        /// </summary>
        /// <param name="profileName">Name of the profile to delete</param>
        void DeleteProfile(string profileName);

        /// <summary>
        /// Retrieves the name of the last used profile from the settings store
        /// </summary>
        /// <returns>Last used profile name or empty string if not set</returns>
        string GetLastProfile();

        /// <summary>
        /// Saves the name of the last used profile to the settings store
        /// </summary>
        /// <param name="lastProfileName">Name of the last used profile to store</param>
        void SetLastProfile(string lastProfileName);
    }

    /// <summary>
    /// Manages AI connection profiles by storing and retrieving settings from the Visual Studio settings store
    /// </summary>
    /// <remarks>
    /// This implementation is not thread-safe. Concurrent access should be synchronized externally.
    /// </remarks>
    public class AiProfileManager : IAiProfileManager
    {
        public IVsSettingsStoreService _settingsStoreService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiProfileManager"/> class
        /// </summary>
        /// <param name="settingsStoreService">
        /// Visual Studio settings store service that handles secure storage operations.
        /// Must implement encryption/decryption for sensitive data like API keys
        /// </param>
        ///  /// <exception cref="ArgumentNullException">Thrown when <paramref name="settingsStoreService"/> is null</exception>
        public AiProfileManager(IVsSettingsStoreService settingsStoreService)
        {
            _settingsStoreService = settingsStoreService ?? throw new ArgumentNullException(nameof(settingsStoreService));
        }

        /// <inheritdoc />
        public List<string> GetProfileIds()
        {
            if (_settingsStoreService.GetStringOrDefault(
                "Profiles",
                string.Empty,
                out string profilesStr) == VSConstants.S_OK)
            {
                return profilesStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return new List<string>();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Encrypts API key. Uses invariant culture for numeric conversions. 
        /// Overwrites existing values for all settings.
        /// </remarks>
        public NeuroAssistantResult SaveProfile(IAiConnectionSettings settings)
        {
            NeuroAssistantResult result = NeuroAssistantResult.Ok;
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var profiles = GetProfileIds();
            if (!profiles.Contains(settings.ProfileName))
            {
                profiles.Add(settings.ProfileName);
                _settingsStoreService.SetString("Profiles", string.Join(";", profiles));
                result = NeuroAssistantResult.AddNewItem;
            }

            _settingsStoreService.SetTagEncryptValue(settings.ProfileName, "ApiKey", settings.ApiKey);
            _settingsStoreService.SetTagValue(settings.ProfileName, "EndpointUrl", settings.EndpointUrl);
            _settingsStoreService.SetTagValue(settings.ProfileName, "Model", settings.Model);
            _settingsStoreService.SetTagValue(settings.ProfileName, "MaxTokens", settings.MaxTokens.ToString());
            _settingsStoreService.SetTagValue(settings.ProfileName, "Temperature", settings.Temperature.ToString(CultureInfo.InvariantCulture));

            return result;
        }

        /// <inheritdoc />
        public IAiConnectionSettings LoadProfile<T>(string profileName) where T : IAiConnectionSettings, new()
        {
            var profiles = GetProfileIds();
            if (!profiles.Contains(profileName))
            {
                throw new KeyNotFoundException($"Profile '{profileName}' not found in settings store");
            }

            try
            {
                if (_settingsStoreService.GetTagDecryptValue(profileName, "ApiKey", out string apiKey) == VSConstants.S_OK &&
                    _settingsStoreService.GetTagValue(profileName, "EndpointUrl", out string endpointUrl) == VSConstants.S_OK &&
                    _settingsStoreService.GetTagValue(profileName, "Model", out string model) == VSConstants.S_OK &&
                    _settingsStoreService.GetTagValue(profileName, "MaxTokens", out string maxTokens) == VSConstants.S_OK &&
                    _settingsStoreService.GetTagValue(profileName, "Temperature", out string temperature) == VSConstants.S_OK)
                {
                    return new T
                    {
                        ProfileName = profileName,
                        ApiKey = apiKey,
                        EndpointUrl = endpointUrl,
                        Model = model,
                        MaxTokens = int.Parse(maxTokens),
                        Temperature = double.Parse(temperature, CultureInfo.InvariantCulture),
                    };
                }

                throw new InvalidOperationException($"Error read from visual studio settings store");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid data format in profile '{profileName}'", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Removes all settings associated with the profile including encrypted data.
        /// No error occurs if the profile doesn't exist.
        /// </remarks>
        public void DeleteProfile(string profileName)
        {
            if (!GetProfileIds().Contains(profileName))
            {
                return;
            }

            var profiles = GetProfileIds();
            profiles.Remove(profileName);
            _settingsStoreService.SetString("Profiles", string.Join(";", profiles));

            string[] keys = { "ApiKey", "EndpointUrl", "MaxTokens", "Model", "Temperature" };

            for (int i = 0; i < keys.Length; i++)
            {
                _settingsStoreService.DeleteProperty(profileName, keys[i]);
            }
        }

        /// <inheritdoc />
        public string GetLastProfile()
        {
            _settingsStoreService.GetStringOrDefault("LastProfile", string.Empty, out string value);
            return value;
        }

        /// <inheritdoc />
        public void SetLastProfile(string lastProfileName)
        {
            _settingsStoreService.SetString("LastProfile", lastProfileName);
        }
    }
}
