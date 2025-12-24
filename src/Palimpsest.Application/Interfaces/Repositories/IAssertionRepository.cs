using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Assertion operations.
/// Assertions are append-only; no update or delete operations.
/// </summary>
public interface IAssertionRepository
{
    Task<Assertion?> GetByIdAsync(Guid assertionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assertion>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assertion>> GetBySubjectEntityIdAsync(Guid universeId, Guid subjectEntityId, CancellationToken cancellationToken = default);
    Task<Assertion> CreateAsync(Assertion assertion, CancellationToken cancellationToken = default);
}
