using Newtonsoft.Json;

namespace NeuroAssistant.Ai.Chat.Response
{
    public class ChatChoice
    {
        public ChatChoice(ChatMessageResponse message)
        {
            Message = message;
        }

        [JsonProperty("message")]
        public ChatMessageResponse Message { get; set; }
    }
}
