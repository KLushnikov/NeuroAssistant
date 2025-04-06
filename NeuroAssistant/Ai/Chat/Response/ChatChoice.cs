using System.Text.Json.Serialization;

namespace NeuroAssistant.Ai.Chat.Response
{
    public class ChatChoice
    {
        public ChatChoice(ChatMessageResponse message)
        {
            Message = message;
        }

        [JsonPropertyName("message")]
        public ChatMessageResponse Message { get; set; }
    }
}
