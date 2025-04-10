using Microsoft.VisualStudio.Shell;
using NeuroAssistant.Ai.Chat.Request;
using NeuroAssistant.Ai.Chat.Response;
using NeuroAssistant.Ai.Connection;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroAssistant.Ai
{
    /// <summary>
    /// Service for handling AI-assisted operations using chat-based AI models
    /// </summary>
    public class AiAssistedService : IDisposable
    {
        private bool _disposed;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IAiConnectionSettings _aiConnection;

        /// <summary>
        /// Initializes a new instance of the AI assisted service
        /// </summary>
        /// <param name="aiConnection">AI connection settings provider</param>
        /// <exception cref="ArgumentNullException">Thrown when aiConnection is null</exception>
        public AiAssistedService(IAiConnectionSettings aiConnection)
        {
            _aiConnection = aiConnection ?? throw new ArgumentNullException(nameof(aiConnection));
        }

        /// <summary>
        /// Processes chat messages and gets AI response
        /// </summary>
        /// <param name="chatMessages">Array of chat messages for context</param>
        /// <returns>Generated response content or error message</returns>
        private async Task<string> GetChatResponseAsync(ChatMessage[] chatMessages,
                                                        CancellationToken cancellationToken = default)
        {
            try
            {
                var requestBody = new ChatRequest(
                    model: _aiConnection.Model,
                    messages: chatMessages,
                    temperature: _aiConnection.Temperature,
                    maxTokens: _aiConnection.MaxTokens
                );

                var response = await SendChatRequestAsync(requestBody, cancellationToken);

                return response?.Trim() ?? "Failed to generate message";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Sends chat request to AI API endpoint
        /// </summary>
        /// <param name="chatRequest">Formatted chat request</param>
        /// <returns>Response content from AI model</returns>
        /// <exception cref="ArgumentNullException">Thrown when chatRequest is null</exception>
        /// <exception cref="InvalidOperationException">Thrown for invalid response format</exception>
        private async Task<string> SendChatRequestAsync(ChatRequest chatRequest,
                                                        CancellationToken cancellationToken = default)
        {
            if (chatRequest == null)
            {
                throw new ArgumentNullException(nameof(chatRequest));
            }

            using (var request = new HttpRequestMessage(HttpMethod.Post, _aiConnection.EndpointUrl))
            {
                if (!string.IsNullOrWhiteSpace(_aiConnection.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _aiConnection.ApiKey);
                }

                request.Content = JsonContent.Create(chatRequest, options: _jsonOptions);

                using (var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var responseData = await JsonSerializer.DeserializeAsync<ChatResponse>(
                        responseStream,
                        options: _jsonOptions);

                    return responseData.Choices?.FirstOrDefault()?.Message?.Content
                        ?? throw new InvalidOperationException("Invalid response format");
                }

            }
        }

        /// <summary>
        /// Generates commit message based on code diff using Conventional Commits format
        /// </summary>
        /// <param name="diff">Code difference content</param>
        /// <returns>Formatted commit message or error information</returns>
        public async Task<string> GenerateCommitMessageAsync(string diff,
                                                             CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(diff))
            {
                return "Error: Empty diff content";
            }

            ChatMessage[] chatMessages = new[]
            {
                new ChatMessage("system","You are a C# expert. Generate a commit message in Conventional Commits format."),
                new ChatMessage("user", $"Analyze this diff:\n\n{diff}")
            };

            return await GetChatResponseAsync(chatMessages, cancellationToken);
        }

        /// <summary>
        /// Disposes managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }
}
