using Palimpsest.Application.Interfaces.Services;

namespace Palimpsest.Infrastructure.Services;

/// <summary>
/// Stub implementation of ILLMProvider for Phase 1.
/// Returns mock data instead of actual LLM calls.
/// TODO: Implement actual Ollama and OpenRouter providers.
/// </summary>
public class StubLLMProvider : ILLMProvider
{
    public string ProviderName => "Stub";

    public Task<string> GenerateCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // TODO: Replace with actual LLM provider implementation
        return Task.FromResult("Stub LLM response");
    }

    public Task<string> GenerateStructuredCompletionAsync(string prompt, string jsonSchema, CancellationToken cancellationToken = default)
    {
        // TODO: Replace with actual LLM provider implementation
        // For now, return a minimal valid JSON structure
        return Task.FromResult("{}");
    }
}
