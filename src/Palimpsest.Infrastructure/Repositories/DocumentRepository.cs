using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly PalimpsestDbContext _context;

    public DocumentRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByUniverseIdAsync(Guid universeId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.UniverseId == universeId)
            .OrderBy(d => d.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document> CreateAsync(Document document, CancellationToken cancellationToken = default)
    {
        document.CreatedAt = DateTime.UtcNow;
        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(documentId, cancellationToken);
        if (document != null)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
