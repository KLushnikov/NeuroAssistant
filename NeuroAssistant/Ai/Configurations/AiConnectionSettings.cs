using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace NeuroAssistant.Ai.Configurations
{
    public class AiConnectionSettings
    {
        public AiConnectionSettings(string endpointUrl,
                                    string apiKey,
                                    string model,
                                    double temperature,
                                    int maxTokens)
        {
            EndpointUrl = endpointUrl;
            ApiKey = apiKey;
            Model = model;
            Temperature = temperature;
            MaxTokens = maxTokens;
        }

        [JsonPropertyName("endpoint_url")]
        public string EndpointUrl { get; private set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; private set; }

        [JsonProperty("model")]
        public string Model { get; private set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.3;

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 200;
    }
}
