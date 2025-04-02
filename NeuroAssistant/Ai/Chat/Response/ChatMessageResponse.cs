using Newtonsoft.Json;

namespace NeuroAssistant.Ai.Chat.Response
{
    public class ChatMessageResponse
    {
        public ChatMessageResponse(string content)
        {
            Content = content;
        }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
