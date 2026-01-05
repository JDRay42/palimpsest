using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Services;

/// <summary>
/// Service for resolving entity mentions to canonical entities.
/// Handles exact matches, ambiguous matches, and creation of new entities.
/// </summary>
public interface IEntityResolutionService
{
    /// <summary>
    /// Resolves an entity mention to a canonical entity.
    /// </summary>
    /// <param name="mention">The entity mention to resolve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved mention with updated EntityId and ResolutionStatus.</returns>
    Task<EntityMention> ResolveMentionAsync(
        EntityMention mention,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves multiple entity mentions in batch.
    /// </summary>
    /// <param name="mentions">The entity mentions to resolve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved mentions with updated EntityId and ResolutionStatus.</returns>
    Task<IEnumerable<EntityMention>> ResolveMentionsBatchAsync(
        IEnumerable<EntityMention> mentions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds candidate entities that might match the given surface form.
    /// </summary>
    /// <param name="universeId">The universe to search within.</param>
    /// <param name="surfaceForm">The text form to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of candidate entities with confidence scores.</returns>
    Task<IEnumerable<(Entity Entity, float Confidence)>> FindCandidateEntitiesAsync(
        Guid universeId,
        string surfaceForm,
        CancellationToken cancellationToken = default);
}
