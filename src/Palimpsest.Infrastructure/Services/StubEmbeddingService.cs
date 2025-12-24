using Palimpsest.Application.Interfaces.Services;

namespace Palimpsest.Infrastructure.Services;

/// <summary>
/// Stub implementation of IEmbeddingService for Phase 1.
/// Returns mock embeddings instead of actual computations.
/// TODO: Implement actual local embedding model.
/// </summary>
public class StubEmbeddingService : IEmbeddingService
{
    public string ModelIdentifier => "stub-embeddings-v1";
    
    public int Dimensions => 384; // Common dimension for sentence transformers

    public Task<float[]> ComputeEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // TODO: Replace with actual embedding computation
        // For now, return a vector of zeros
        var embedding = new float[Dimensions];
        return Task.FromResult(embedding);
    }
}
