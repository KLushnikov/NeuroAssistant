using System.Text.Json.Serialization;

namespace NeuroAssistant.Ai.Chat.Request
{
    public class ChatMessage
    {
        public ChatMessage(string role,
                           string content)
        {
            Role = role;
            Content = content;
        }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
