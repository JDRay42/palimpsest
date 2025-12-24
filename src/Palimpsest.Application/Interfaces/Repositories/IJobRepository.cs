using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Job operations.
/// </summary>
public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetByStatusAsync(Guid universeId, JobStatus status, CancellationToken cancellationToken = default);
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    Task UpdateAsync(Job job, CancellationToken cancellationToken = default);
}
