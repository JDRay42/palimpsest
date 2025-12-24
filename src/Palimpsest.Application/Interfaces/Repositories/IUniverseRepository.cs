using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Universe operations.
/// </summary>
public interface IUniverseRepository
{
    /// <summary>
    /// Gets a universe by its unique identifier.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The universe if found; otherwise, null.</returns>
    Task<Universe?> GetByIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a universe by its name.
    /// </summary>
    /// <param name="name">The name of the universe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The universe if found; otherwise, null.</returns>
    Task<Universe?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all universes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all universes.</returns>
    Task<IEnumerable<Universe>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new universe.
    /// </summary>
    /// <param name="universe">The universe to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created universe.</returns>
    Task<Universe> CreateAsync(Universe universe, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing universe.
    /// </summary>
    /// <param name="universe">The universe to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Universe universe, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a universe by its unique identifier.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid universeId, CancellationToken cancellationToken = default);
}
