using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Services;

/// <summary>
/// Service for document ingestion pipeline.
/// Handles text normalization, segmentation, and initial entity/assertion extraction.
/// </summary>
public interface IIngestionService
{
    /// <summary>
    /// Ingests a document into the universe.
    /// Creates document version, segments, and initiates extraction pipeline.
    /// </summary>
    Task<Guid> IngestDocumentAsync(
        Guid universeId,
        Guid documentId,
        string rawText,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Segments text into chapter/section/paragraph units.
    /// </summary>
    Task<IEnumerable<Segment>> SegmentTextAsync(
        Guid versionId,
        string normalizedText,
        CancellationToken cancellationToken = default);
}
