using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository for managing entity mentions in the database.
/// </summary>
public interface IEntityMentionRepository
{
    /// <summary>
    /// Creates a new entity mention.
    /// </summary>
    Task<EntityMention> CreateAsync(EntityMention mention, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple entity mentions in batch.
    /// </summary>
    Task CreateRangeAsync(IEnumerable<EntityMention> mentions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity mention.
    /// </summary>
    Task UpdateAsync(EntityMention mention, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity mention by ID.
    /// </summary>
    Task<EntityMention?> GetByIdAsync(Guid mentionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all mentions for a specific segment.
    /// </summary>
    Task<IEnumerable<EntityMention>> GetBySegmentIdAsync(Guid segmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all mentions for a specific entity.
    /// </summary>
    Task<IEnumerable<EntityMention>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all unresolved or candidate mentions in a universe.
    /// </summary>
    Task<IEnumerable<EntityMention>> GetUnresolvedMentionsAsync(Guid universeId, CancellationToken cancellationToken = default);
}
