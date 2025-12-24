using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Entity operations within a universe.
/// </summary>
public interface IEntityRepository
{
    Task<Entity?> GetByIdAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entity>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    Task<Entity?> GetByCanonicalNameAsync(Guid universeId, string canonicalName, CancellationToken cancellationToken = default);
    Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Entity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid entityId, CancellationToken cancellationToken = default);
}
