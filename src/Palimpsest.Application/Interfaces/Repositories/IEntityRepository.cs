using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Entity operations within a universe.
/// </summary>
public interface IEntityRepository
{
    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<Entity?> GetByIdAsync(Guid entityId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities in a universe.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of entities in the universe.</returns>
    Task<IEnumerable<Entity>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an entity by its canonical name within a universe.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="canonicalName">The canonical name of the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<Entity?> GetByCanonicalNameAsync(Guid universeId, string canonicalName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created entity.</returns>
    Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Entity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid entityId, CancellationToken cancellationToken = default);
}
