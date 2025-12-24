using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

public class AssertionRepository : IAssertionRepository
{
    private readonly PalimpsestDbContext _context;

    public AssertionRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<Assertion?> GetByIdAsync(Guid assertionId, CancellationToken cancellationToken = default)
    {
        return await _context.Assertions
            .Include(a => a.SubjectEntity)
            .Include(a => a.ObjectEntity)
            .Include(a => a.EvidenceSegment)
            .FirstOrDefaultAsync(a => a.AssertionId == assertionId, cancellationToken);
    }

    public async Task<IEnumerable<Assertion>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default)
    {
        return await _context.Assertions
            .Where(a => a.UniverseId == universeId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assertion>> GetBySubjectEntityIdAsync(Guid universeId, Guid subjectEntityId, CancellationToken cancellationToken = default)
    {
        return await _context.Assertions
            .Where(a => a.UniverseId == universeId && a.SubjectEntityId == subjectEntityId)
            .OrderByDescending(a => a.Confidence)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Assertion> CreateAsync(Assertion assertion, CancellationToken cancellationToken = default)
    {
        assertion.CreatedAt = DateTime.UtcNow;
        _context.Assertions.Add(assertion);
        await _context.SaveChangesAsync(cancellationToken);
        return assertion;
    }
}
