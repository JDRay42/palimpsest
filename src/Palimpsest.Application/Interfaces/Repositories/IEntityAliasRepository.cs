using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository for managing entity aliases in the database.
/// </summary>
public interface IEntityAliasRepository
{
    /// <summary>
    /// Creates a new entity alias.
    /// </summary>
    Task<EntityAlias> CreateAsync(EntityAlias alias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all aliases for a specific entity.
    /// </summary>
    Task<IEnumerable<EntityAlias>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities with exact normalized alias match.
    /// </summary>
    /// <param name="aliasNorm">The normalized alias to match.</param>
    /// <param name="universeId">The universe to search within.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entities with matching aliases and confidence = 1.0.</returns>
    Task<IEnumerable<(Entity Entity, float Confidence)>> FindExactMatchAsync(
        string aliasNorm,
        Guid universeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities with similar aliases using fuzzy matching (pg_trgm).
    /// </summary>
    /// <param name="surfaceForm">The text to match against aliases.</param>
    /// <param name="universeId">The universe to search within.</param>
    /// <param name="similarityThreshold">Minimum similarity score (0.0 to 1.0, default 0.75).</param>
    /// <param name="maxResults">Maximum number of results to return (default 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entities with similar aliases and their similarity scores.</returns>
    Task<IEnumerable<(Entity Entity, float Confidence)>> FindSimilarMatchesAsync(
        string surfaceForm,
        Guid universeId,
        float similarityThreshold = 0.75f,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an alias already exists for an entity.
    /// </summary>
    Task<bool> AliasExistsAsync(Guid entityId, string aliasNorm, CancellationToken cancellationToken = default);
}
