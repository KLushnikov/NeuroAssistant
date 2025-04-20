using Newtonsoft.Json;

namespace NeuroAssistant.Ai.Connection
{
    /// <summary>
    /// Represents AI connection settings configuration
    /// </summary>
    public class AiConnectionSettings : IAiConnectionSettings
    {
        [JsonProperty("profile_name")]
        public string ProfileName { get; set; }

        [JsonProperty("endpoint_url")]
        public string EndpointUrl { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.3;

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 200;
    }
}
