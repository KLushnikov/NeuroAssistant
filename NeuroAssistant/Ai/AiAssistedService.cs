using Microsoft.VisualStudio.Shell;
using NeuroAssistant.Ai.Chat.Request;
using NeuroAssistant.Ai.Chat.Response;
using NeuroAssistant.Ai.Configurations;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private readonly AiConnectionSettings _aiConnection;

        public AiAssistedService(AiConnectionSettings aiConnection)
        {
            _aiConnection = aiConnection;
        }

        private async Task<string> GetChatResponseAsync(ChatMessage[] chatMessages)
        {
            try
            {
                var requestBody = new ChatRequest(
                    model: _aiConnection.Model,
                    messages: chatMessages,
                    temperature: _aiConnection.Temperature,
                    maxTokens: _aiConnection.MaxTokens
                );

                var response = await SendChatRequestAsync(requestBody);

                return response?.Trim() ?? "Failed to generate message";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> SendChatRequestAsync(ChatRequest chatRequest)
        {
            if (chatRequest == null)
            {
                throw new ArgumentNullException(nameof(chatRequest));
            }

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, _aiConnection.EndpointUrl))
                {
                    if (!string.IsNullOrEmpty(_aiConnection.ApiKey))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _aiConnection.ApiKey);
                    }

                    request.Content = JsonContent.Create(chatRequest, options: _jsonOptions);

                    using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        var responseData = await JsonSerializer.DeserializeAsync<ChatResponse>(
                            responseStream,
                            options: _jsonOptions).ConfigureAwait(false);

                        return responseData.Choices?.FirstOrDefault()?.Message?.Content
                            ?? throw new InvalidOperationException("Invalid response format");
                    }

                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"API Error: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to parse API response", ex);
            }

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
