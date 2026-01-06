using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

/// <summary>
/// Repository for managing entity mentions in the database.
/// </summary>
public class EntityMentionRepository : IEntityMentionRepository
{
    private readonly PalimpsestDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityMentionRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public EntityMentionRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<EntityMention> CreateAsync(EntityMention mention, CancellationToken cancellationToken = default)
    {
        _context.EntityMentions.Add(mention);
        await _context.SaveChangesAsync(cancellationToken);
        return mention;
    }

    public async Task CreateRangeAsync(IEnumerable<EntityMention> mentions, CancellationToken cancellationToken = default)
    {
        _context.EntityMentions.AddRange(mentions);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EntityMention mention, CancellationToken cancellationToken = default)
    {
        _context.EntityMentions.Update(mention);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EntityMention?> GetByIdAsync(Guid mentionId, CancellationToken cancellationToken = default)
    {
        return await _context.EntityMentions
            .Include(m => m.Entity)
            .Include(m => m.Segment)
            .FirstOrDefaultAsync(m => m.MentionId == mentionId, cancellationToken);
    }

    public async Task<IEnumerable<EntityMention>> GetBySegmentIdAsync(Guid segmentId, CancellationToken cancellationToken = default)
    {
        return await _context.EntityMentions
            .Include(m => m.Entity)
            .Where(m => m.SegmentId == segmentId)
            .OrderBy(m => m.SpanStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EntityMention>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.EntityMentions
            .Include(m => m.Segment)
            .Where(m => m.EntityId == entityId)
            .OrderBy(m => m.Segment.Ordinal)
            .ThenBy(m => m.SpanStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EntityMention>> GetUnresolvedMentionsAsync(Guid universeId, CancellationToken cancellationToken = default)
    {
        return await _context.EntityMentions
            .Include(m => m.Segment)
            .Where(m => m.UniverseId == universeId &&
                       (m.ResolutionStatus == ResolutionStatus.Unresolved || m.ResolutionStatus == ResolutionStatus.Candidate))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
