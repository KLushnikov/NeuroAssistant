using Newtonsoft.Json;

namespace NeuroAssistant.Ai.Chat.Request
{
    public class ChatRequest
    {
        public ChatRequest(string model,
                           ChatMessage[] messages,
                           double temperature,
                           int maxTokens)
        {
            Model = model;
            Messages = messages;
            Temperature = temperature;
            MaxTokens = maxTokens;
        }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public ChatMessage[] Messages { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }
    }
}
