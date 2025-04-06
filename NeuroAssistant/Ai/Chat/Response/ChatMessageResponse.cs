using System.Text.Json.Serialization;

namespace NeuroAssistant.Ai.Chat.Response
{
    public class ChatMessageResponse
    {
        public ChatMessageResponse(string content)
        {
            Content = content;
        }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
