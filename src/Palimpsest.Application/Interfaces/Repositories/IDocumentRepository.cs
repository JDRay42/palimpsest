using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Document operations.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Gets a document by its unique identifier.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document if found; otherwise, null.</returns>
    Task<Document?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all documents in a universe.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of documents in the universe.</returns>
    Task<IEnumerable<Document>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <param name="document">The document to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created document.</returns>
    Task<Document> CreateAsync(Document document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing document.
    /// </summary>
    /// <param name="document">The document to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a document by its unique identifier.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default);
}
