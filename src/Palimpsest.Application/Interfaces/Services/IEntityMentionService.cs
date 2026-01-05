using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Services;

/// <summary>
/// Service for detecting entity mentions in text segments.
/// Identifies capitalized sequences and other patterns that may represent named entities.
/// </summary>
public interface IEntityMentionService
{
    /// <summary>
    /// Scans a segment and detects potential entity mentions.
    /// </summary>
    /// <param name="segment">The segment to scan for entity mentions.</param>
    /// <param name="universeId">The universe ID to associate with detected mentions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of detected entity mention candidates.</returns>
    Task<IEnumerable<EntityMention>> DetectMentionsAsync(
        Segment segment,
        Guid universeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes multiple segments in batch to detect entity mentions.
    /// </summary>
    /// <param name="segments">The segments to process.</param>
    /// <param name="universeId">The universe ID to associate with detected mentions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of detected entity mentions across all segments.</returns>
    Task<IEnumerable<EntityMention>> DetectMentionsBatchAsync(
        IEnumerable<Segment> segments,
        Guid universeId,
        CancellationToken cancellationToken = default);
}
