using Microsoft.VisualStudio;
using NeuroAssistant.Ai.Connection;
using NeuroAssistant.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NeuroAssistant.Ai.Profiles
{
    /// <summary>
    /// Manages AI connection profiles by storing and retrieving settings from the Visual Studio settings store
    /// </summary>
    internal class AiProfileManager
    {
        public IVsSettingsStoreService _settingsStoreService;

        /// <summary>
        /// Initializes a new instance of the AiProfileManager class
        /// </summary>
        /// <param name="settingsStoreService">Visual Studio settings store service</param>
        public AiProfileManager(IVsSettingsStoreService settingsStoreService)
        {
            _settingsStoreService = settingsStoreService;
        }

        /// <summary>
        /// Retrieves all stored profile names from the settings store
        /// </summary>
        /// <returns>List of profile names. Returns empty list if no profiles exist</returns>
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

        /// <summary>
        /// Saves or updates a connection profile in the settings store
        /// </summary>
        /// <param name="settings">Profile settings to persist</param>
        /// <exception cref="ArgumentNullException">Thrown if settings are null</exception>
        /// <remarks>
        /// Encrypts and stores API key. Uses invariant culture for numeric conversions.
        /// </remarks>
        public void SaveProfile(IAiConnectionSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var profiles = GetProfileIds();
            if (!profiles.Contains(settings.ProfileName))
            {
                profiles.Add(settings.ProfileName);
                _settingsStoreService.SetString("Profiles", string.Join(";", profiles));
            }

            _settingsStoreService.SetTagEncryptValue(settings.ProfileName, "ApiKey", settings.ApiKey);
            _settingsStoreService.SetTagValue(settings.ProfileName, "EndpointUrl", settings.EndpointUrl);
            _settingsStoreService.SetTagValue(settings.ProfileName, "Model", settings.Model);
            _settingsStoreService.SetTagValue(settings.ProfileName, "MaxTokens", settings.MaxTokens.ToString());
            _settingsStoreService.SetTagValue(settings.ProfileName, "Temperature", settings.Temperature.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Loads a specific profile from the settings store
        /// </summary>
        /// <typeparam name="T">Type implementing IAiConnectionSettings with parameterless constructor</typeparam>
        /// <param name="profileName">Name of the profile to load</param>
        /// <returns>Initialized settings object</returns>
        /// <exception cref="KeyNotFoundException">Thrown if profile doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown for invalid/missing data or parsing errors</exception>
        public IAiConnectionSettings LoadProfile<T>(string profileName) where T : IAiConnectionSettings, new()
        {
            if (!GetProfileIds().Contains(profileName))
            {
                throw new KeyNotFoundException($"Profile '{profileName}' not found");
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
                        Temperature = double.Parse(temperature),
                    };
                }

                throw new InvalidOperationException($"Error read from visual studio settings store");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid data format in profile '{profileName}'", ex);
            }
        }

        /// <summary>
        /// Removes a profile and its associated settings
        /// </summary>
        /// <param name="profileName">Name of the profile to delete</param>
        /// <remarks>
        /// Deletes all settings associated with the profile including encrypted API key
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
    }
}
