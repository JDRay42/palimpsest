using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

/// <summary>
/// Repository for managing questionable items in the database.
/// </summary>
public class QuestionableItemRepository : IQuestionableItemRepository
{
    private readonly PalimpsestDbContext _context;

    public QuestionableItemRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<QuestionableItem> CreateAsync(QuestionableItem item, CancellationToken cancellationToken = default)
    {
        _context.QuestionableItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<QuestionableItem?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _context.QuestionableItems
            .Include(q => q.SubjectEntity)
            .FirstOrDefaultAsync(q => q.ItemId == itemId, cancellationToken);
    }

    public async Task<IEnumerable<QuestionableItem>> GetByUniverseIdAsync(
        Guid universeId,
        QuestionableItemStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.QuestionableItems
            .Include(q => q.SubjectEntity)
            .Where(q => q.UniverseId == universeId);

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        return await query
            .OrderByDescending(q => q.Severity)
            .ThenBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(QuestionableItem item, CancellationToken cancellationToken = default)
    {
        _context.QuestionableItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
