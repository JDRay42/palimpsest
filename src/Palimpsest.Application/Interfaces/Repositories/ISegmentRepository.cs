using Palimpsest.Domain.Entities;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Segment operations.
/// </summary>
public interface ISegmentRepository
{
    Task<Segment?> GetByIdAsync(Guid segmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Segment>> GetByVersionIdAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<Segment> CreateAsync(Segment segment, CancellationToken cancellationToken = default);
    Task<IEnumerable<Segment>> CreateRangeAsync(IEnumerable<Segment> segments, CancellationToken cancellationToken = default);
}
