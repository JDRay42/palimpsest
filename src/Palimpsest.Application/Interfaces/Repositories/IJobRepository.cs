using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Job operations.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Gets a job by its unique identifier.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The job if found; otherwise, null.</returns>
    Task<Job?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all jobs in a universe.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of jobs in the universe.</returns>
    Task<IEnumerable<Job>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all jobs in a universe with a specific status.
    /// </summary>
    /// <param name="universeId">The unique identifier of the universe.</param>
    /// <param name="status">The job status to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of jobs with the specified status.</returns>
    Task<IEnumerable<Job>> GetByStatusAsync(Guid universeId, JobStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new job.
    /// </summary>
    /// <param name="job">The job to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created job.</returns>
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing job.
    /// </summary>
    /// <param name="job">The job to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Job job, CancellationToken cancellationToken = default);
}
