using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

public class UniverseRepository : IUniverseRepository
{
    private readonly PalimpsestDbContext _context;

    public UniverseRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<Universe?> GetByIdAsync(Guid universeId, CancellationToken cancellationToken = default)
    {
        return await _context.Universes
            .FirstOrDefaultAsync(u => u.UniverseId == universeId, cancellationToken);
    }

    public async Task<Universe?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Universes
            .FirstOrDefaultAsync(u => u.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Universe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Universes
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Universe> CreateAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        universe.CreatedAt = DateTime.UtcNow;
        _context.Universes.Add(universe);
        await _context.SaveChangesAsync(cancellationToken);
        return universe;
    }

    public async Task UpdateAsync(Universe universe, CancellationToken cancellationToken = default)
    {
        _context.Universes.Update(universe);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid universeId, CancellationToken cancellationToken = default)
    {
        var universe = await GetByIdAsync(universeId, cancellationToken);
        if (universe != null)
        {
            _context.Universes.Remove(universe);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
