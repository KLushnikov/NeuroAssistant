using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeuroAssistant.Ai.Chat.Response
{
    public class ChatResponse
    {
        public ChatResponse(List<ChatChoice> choices)
        {
            Choices = choices;
        }

        [JsonPropertyName("choices")]
        public List<ChatChoice> Choices { get; set; }
    }
}
