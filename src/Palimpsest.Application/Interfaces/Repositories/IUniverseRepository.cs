using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Universe operations.
/// </summary>
public interface IUniverseRepository
{
    Task<Universe?> GetByIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    Task<Universe?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Universe>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Universe> CreateAsync(Universe universe, CancellationToken cancellationToken = default);
    Task UpdateAsync(Universe universe, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid universeId, CancellationToken cancellationToken = default);
}
