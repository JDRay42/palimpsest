namespace Palimpsest.Application.Interfaces.Services;

/// <summary>
/// Interface for embedding computation.
/// MVP supports local embedding models.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Computes an embedding vector for the given text.
    /// </summary>
    Task<float[]> ComputeEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the embedding model identifier.
    /// </summary>
    string ModelIdentifier { get; }
    
    /// <summary>
    /// Gets the dimensionality of the embedding vectors produced.
    /// </summary>
    int Dimensions { get; }
}
