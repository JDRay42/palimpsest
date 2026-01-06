using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Job operations.
/// Provides data access for background processing jobs.
/// </summary>
public class JobRepository : IJobRepository
{
    private readonly PalimpsestDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public JobRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .FirstOrDefaultAsync(j => j.JobId == jobId, cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(j => j.UniverseId == universeId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetByStatusAsync(Guid universeId, JobStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(j => j.UniverseId == universeId && j.Status == status)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        job.CreatedAt = DateTime.UtcNow;
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
