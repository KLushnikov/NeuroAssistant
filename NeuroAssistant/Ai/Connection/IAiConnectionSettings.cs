namespace NeuroAssistant.Ai.Connection
{
    public interface IAiConnectionSettings
    {
        /// <summary>
        /// Gets or sets the display name for AI connection profile
        /// </summary>
        string ProfileName { get; set; }
        /// <summary>
        /// Gets or sets the secret API key for authentication
        /// </summary>
        string ApiKey { get; set; }
        /// <summary>
        /// Gets or sets the API endpoint URL for service access
        /// </summary>
        string EndpointUrl { get; set; }
        /// <summary>
        /// Gets or sets the AI model name/version to use
        /// (Examples: "local-model")
        /// </summary>
        string Model { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of tokens allowed in response
        /// (Typical range: 1-4096)
        /// </summary>
        int MaxTokens { get; set; }
        /// <summary>
        /// Gets or sets the temperature parameter for response generation
        /// (0.0 - strict/deterministic output, 1.0 - creative/random)
        /// Recommended values:
        /// - 0.0-0.3: For tasks with unambiguous answers (facts, Q&A)
        /// - 0.4-0.7: Balance between creativity and accuracy (common default range)
        /// - 0.8-1.0: High creativity (poetry, storytelling)
        /// - >1.0: Extreme randomness (may produce nonsense)
        /// </summary>
        double Temperature { get; set; }
    }
}