using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Assertion operations.
/// Assertions are append-only; no update or delete operations.
/// </summary>
public interface IAssertionRepository
{
    /// <summary>
    /// Gets an assertion by its unique identifier.
    /// </summary>
    /// <param name="assertionId">The unique identifier of the assertion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The assertion if found; otherwise, null.</returns>
    Task<Assertion?> GetByIdAsync(Guid assertionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all assertions in a universe.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of assertions in the universe.</returns>
    Task<IEnumerable<Assertion>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all assertions where the specified entity is the subject.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="subjectEntityId">The unique identifier of the subject entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of assertions with the specified subject entity.</returns>
    Task<IEnumerable<Assertion>> GetBySubjectEntityIdAsync(Guid universeId, Guid subjectEntityId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new assertion.
    /// </summary>
    /// <param name="assertion">The assertion to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created assertion.</returns>
    Task<Assertion> CreateAsync(Assertion assertion, CancellationToken cancellationToken = default);
}
