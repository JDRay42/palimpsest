using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

public class SegmentRepository : ISegmentRepository
{
    private readonly PalimpsestDbContext _context;

    public SegmentRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<Segment?> GetByIdAsync(Guid segmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Segments
            .Include(s => s.Version)
            .FirstOrDefaultAsync(s => s.SegmentId == segmentId, cancellationToken);
    }

    public async Task<IEnumerable<Segment>> GetByVersionIdAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        return await _context.Segments
            .Where(s => s.VersionId == versionId)
            .OrderBy(s => s.Ordinal)
            .ToListAsync(cancellationToken);
    }

    public async Task<Segment> CreateAsync(Segment segment, CancellationToken cancellationToken = default)
    {
        segment.CreatedAt = DateTime.UtcNow;
        _context.Segments.Add(segment);
        await _context.SaveChangesAsync(cancellationToken);
        return segment;
    }

    public async Task<IEnumerable<Segment>> CreateRangeAsync(IEnumerable<Segment> segments, CancellationToken cancellationToken = default)
    {
        var segmentList = segments.ToList();
        foreach (var segment in segmentList)
        {
            segment.CreatedAt = DateTime.UtcNow;
        }
        
        _context.Segments.AddRange(segmentList);
        await _context.SaveChangesAsync(cancellationToken);
        return segmentList;
    }
}
