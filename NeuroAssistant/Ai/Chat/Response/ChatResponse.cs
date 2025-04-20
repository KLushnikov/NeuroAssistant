using Newtonsoft.Json;
using System.Collections.Generic;

namespace NeuroAssistant.Ai.Chat.Response
{
    public class ChatResponse
    {
        public ChatResponse(List<ChatChoice> choices)
        {
            Choices = choices;
        }

        [JsonProperty("choices")]
        public List<ChatChoice> Choices { get; set; }
    }
}
