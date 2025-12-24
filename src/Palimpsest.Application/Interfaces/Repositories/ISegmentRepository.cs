using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Segment operations.
/// </summary>
public interface ISegmentRepository
{
    /// <summary>
    /// Gets a segment by its unique identifier.
    /// </summary>
    /// <param name="segmentId">The unique identifier of the segment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The segment if found; otherwise, null.</returns>
    Task<Segment?> GetByIdAsync(Guid segmentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all segments in a document version.
    /// </summary>
    /// <param name="versionId">The unique identifier of the document version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of segments in the version.</returns>
    Task<IEnumerable<Segment>> GetByVersionIdAsync(Guid versionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new segment.
    /// </summary>
    /// <param name="segment">The segment to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created segment.</returns>
    Task<Segment> CreateAsync(Segment segment, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates multiple segments in a single operation.
    /// </summary>
    /// <param name="segments">The segments to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created segments.</returns>
    Task<IEnumerable<Segment>> CreateRangeAsync(IEnumerable<Segment> segments, CancellationToken cancellationToken = default);
}
