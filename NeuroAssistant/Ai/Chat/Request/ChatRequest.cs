using System.Text.Json.Serialization;

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

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public ChatMessage[] Messages { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }
}
