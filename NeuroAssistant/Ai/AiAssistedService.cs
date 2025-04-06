using NeuroAssistant.Ai.Chat.Request;
using NeuroAssistant.Ai.Chat.Response;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeuroAssistant.Ai
{
    public class AiAssistedService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly string _lmUrl, _lmModel;
        private readonly double _temperature = 0.3;
        private readonly int _maxTokens = 200;

        public AiAssistedService(string lmUrl, string lmModel, double temperature = 0.3, int maxTokens = 200)
        {
            _lmUrl = lmUrl;
            _lmModel = lmModel;
            _temperature = temperature;
            _maxTokens = maxTokens;
        }

        private async Task<string> GetChatResponseAsync(ChatMessage[] chatMessages)
        {
            try
            {
                var requestBody = new ChatRequest(
                    model: _lmModel,
                    messages: chatMessages,
                    temperature: _temperature,
                    maxTokens: _maxTokens
                );

                var response = await SendChatRequestAsync(requestBody);

                return response?.Trim() ?? "Failed to generate message";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> SendChatRequestAsync(ChatRequest request)
        {
            var jContext = JsonContent.Create(request, options: _jsonOptions);
            var response = await _httpClient.PostAsync(_lmUrl, jContext);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            var responseData = await JsonSerializer.DeserializeAsync<ChatResponse>(responseStream,
                                                                                   options: _jsonOptions);

            return responseData?.Choices?.FirstOrDefault()?.Message?.Content;
        }

        public async Task<string> GenerateCommitMessageAsync(string diff)
        {
            ChatMessage[] chatMessages = new[]
            {
                new ChatMessage("system","You are a C# expert. Generate a commit message in Conventional Commits format."),
                new ChatMessage("user", $"Analyze this diff:\n\n{diff}")
            };

            return await GetChatResponseAsync(chatMessages);
        }
    }
}
