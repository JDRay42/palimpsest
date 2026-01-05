using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Palimpsest.Infrastructure.Data;
using Palimpsest.Infrastructure.Repositories;

namespace Palimpsest.Tests.Unit.Repositories;

public class DocumentRepositoryTests : IDisposable
{
    private readonly PalimpsestDbContext _context;
    private readonly DocumentRepository _repository;
    private readonly Guid _universeId;

    public DocumentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PalimpsestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new PalimpsestDbContext(options);
        _repository = new DocumentRepository(_context);

        // Create a universe for testing
        _universeId = Guid.NewGuid();
        _context.Universes.Add(new Universe
        {
            UniverseId = _universeId,
            Name = "Test Universe",
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDocument_ReturnsDocumentWithVersions()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            DocumentId = documentId,
            UniverseId = _universeId,
            Title = "Test Document",
            Subtype = DocumentSubtype.Book,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result!.DocumentId.Should().Be(documentId);
        result.Title.Should().Be("Test Document");
    }

    [Fact]
    public async Task GetByUniverseIdAsync_ReturnsDocumentsOrderedByTitle()
    {
        // Arrange
        var documents = new[]
        {
            new Document { DocumentId = Guid.NewGuid(), UniverseId = _universeId, Title = "Zeta", CreatedAt = DateTime.UtcNow },
            new Document { DocumentId = Guid.NewGuid(), UniverseId = _universeId, Title = "Alpha", CreatedAt = DateTime.UtcNow },
            new Document { DocumentId = Guid.NewGuid(), UniverseId = _universeId, Title = "Beta", CreatedAt = DateTime.UtcNow }
        };
        await _context.Documents.AddRangeAsync(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByUniverseIdAsync(_universeId)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Alpha");
        result[1].Title.Should().Be("Beta");
        result[2].Title.Should().Be("Zeta");
    }

    [Fact]
    public async Task CreateAsync_ValidDocument_CreatesAndReturnsDocument()
    {
        // Arrange
        var document = new Document
        {
            DocumentId = Guid.NewGuid(),
            UniverseId = _universeId,
            Title = "New Document",
            Subtype = DocumentSubtype.Book,
            SeriesName = "Test Series",
            BookNumber = 1
        };

        // Act
        var result = await _repository.CreateAsync(document);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().Be(document.DocumentId);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        var savedDocument = await _context.Documents.FindAsync(document.DocumentId);
        savedDocument.Should().NotBeNull();
        savedDocument!.SeriesName.Should().Be("Test Series");
        savedDocument.BookNumber.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ExistingDocument_UpdatesDocument()
    {
        // Arrange
        var document = new Document
        {
            DocumentId = Guid.NewGuid(),
            UniverseId = _universeId,
            Title = "Original Title",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        document.Title = "Updated Title";
        document.SeriesName = "New Series";
        await _repository.UpdateAsync(document);

        // Assert
        var updated = await _context.Documents.FindAsync(document.DocumentId);
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated Title");
        updated.SeriesName.Should().Be("New Series");
    }

    [Fact]
    public async Task DeleteAsync_ExistingDocument_RemovesDocument()
    {
        // Arrange
        var document = new Document
        {
            DocumentId = Guid.NewGuid(),
            UniverseId = _universeId,
            Title = "To Delete",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(document.DocumentId);

        // Assert
        var deleted = await _context.Documents.FindAsync(document.DocumentId);
        deleted.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
