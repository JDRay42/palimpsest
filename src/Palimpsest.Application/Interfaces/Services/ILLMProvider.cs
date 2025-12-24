namespace Palimpsest.Application.Interfaces.Services;

/// <summary>
/// Interface for LLM provider implementations.
/// Supports local models (Ollama) and OpenRouter with provider-agnostic architecture.
/// </summary>
public interface ILLMProvider
{
    /// <summary>
    /// Generates a completion based on the provided prompt.
    /// </summary>
    Task<string> GenerateCompletionAsync(string prompt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates a structured JSON response based on the provided prompt and schema.
    /// </summary>
    Task<string> GenerateStructuredCompletionAsync(string prompt, string jsonSchema, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the provider name (e.g., "Ollama", "OpenRouter").
    /// </summary>
    string ProviderName { get; }
}
