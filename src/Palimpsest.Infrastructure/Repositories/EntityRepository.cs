using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

public class EntityRepository : IEntityRepository
{
    private readonly PalimpsestDbContext _context;

    public EntityRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<Entity?> GetByIdAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.Entities
            .Include(e => e.Aliases)
            .FirstOrDefaultAsync(e => e.EntityId == entityId, cancellationToken);
    }

    public async Task<IEnumerable<Entity>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default)
    {
        return await _context.Entities
            .Where(e => e.UniverseId == universeId)
            .OrderBy(e => e.CanonicalName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Entity?> GetByCanonicalNameAsync(Guid universeId, string canonicalName, CancellationToken cancellationToken = default)
    {
        return await _context.Entities
            .FirstOrDefaultAsync(e => e.UniverseId == universeId && e.CanonicalName == canonicalName, cancellationToken);
    }

    public async Task<IEnumerable<Entity>> SearchByNameAsync(Guid universeId, string query, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.ToLower();
        return await _context.Entities
            .Where(e => e.UniverseId == universeId && 
                       (e.CanonicalName.ToLower().Contains(normalizedQuery) ||
                        e.Aliases.Any(a => a.AliasNorm.Contains(normalizedQuery))))
            .Include(e => e.Aliases)
            .OrderBy(e => e.CanonicalName)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Entities.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Entity entity, CancellationToken cancellationToken = default)
    {
        _context.Entities.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(entityId, cancellationToken);
        if (entity != null)
        {
            _context.Entities.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
