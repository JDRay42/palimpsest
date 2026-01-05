using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;
using Palimpsest.Infrastructure.Repositories;

namespace Palimpsest.Tests.Unit.Repositories;

public class UniverseRepositoryTests : IDisposable
{
    private readonly PalimpsestDbContext _context;
    private readonly UniverseRepository _repository;

    public UniverseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PalimpsestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new PalimpsestDbContext(options);
        _repository = new UniverseRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUniverse_ReturnsUniverse()
    {
        // Arrange
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "Test Universe",
            AuthorName = "Test Author",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Universes.AddAsync(universe);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(universe.UniverseId);

        // Assert
        result.Should().NotBeNull();
        result!.UniverseId.Should().Be(universe.UniverseId);
        result.Name.Should().Be("Test Universe");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUniverse_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ExistingName_ReturnsUniverse()
    {
        // Arrange
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "Unique Name",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Universes.AddAsync(universe);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Unique Name");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Unique Name");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUniversesOrderedByName()
    {
        // Arrange
        var universes = new[]
        {
            new Universe { UniverseId = Guid.NewGuid(), Name = "Zeta", CreatedAt = DateTime.UtcNow },
            new Universe { UniverseId = Guid.NewGuid(), Name = "Alpha", CreatedAt = DateTime.UtcNow },
            new Universe { UniverseId = Guid.NewGuid(), Name = "Beta", CreatedAt = DateTime.UtcNow }
        };
        await _context.Universes.AddRangeAsync(universes);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Beta");
        result[2].Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task CreateAsync_ValidUniverse_CreatesAndReturnsUniverse()
    {
        // Arrange
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "New Universe",
            AuthorName = "New Author"
        };

        // Act
        var result = await _repository.CreateAsync(universe);

        // Assert
        result.Should().NotBeNull();
        result.UniverseId.Should().Be(universe.UniverseId);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        var savedUniverse = await _context.Universes.FindAsync(universe.UniverseId);
        savedUniverse.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingUniverse_UpdatesUniverse()
    {
        // Arrange
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "Original Name",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Universes.AddAsync(universe);
        await _context.SaveChangesAsync();

        // Act
        universe.Name = "Updated Name";
        universe.Description = "New Description";
        await _repository.UpdateAsync(universe);

        // Assert
        var updated = await _context.Universes.FindAsync(universe.UniverseId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task DeleteAsync_ExistingUniverse_RemovesUniverse()
    {
        // Arrange
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "To Delete",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Universes.AddAsync(universe);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(universe.UniverseId);

        // Assert
        var deleted = await _context.Universes.FindAsync(universe.UniverseId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentUniverse_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
