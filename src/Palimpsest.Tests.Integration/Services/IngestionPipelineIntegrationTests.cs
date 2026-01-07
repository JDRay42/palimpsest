using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Tests.Integration.Services;

/// <summary>
/// Integration tests for the Phase 2 NER services.
/// Tests entity mention detection, resolution, and alias matching with database.
/// 
/// NOTE: These tests currently SKIPPED because they require a real PostgreSQL database.
/// The in-memory database used by PalimpsestWebApplicationFactory does not support:
/// - pgvector (Vector type for SegmentEmbedding.Embedding)
/// - pg_trgm (fuzzy string matching for EntityAliasRepository)
/// 
/// To run these tests, update PalimpsestWebApplicationFactory to use a test PostgreSQL instance.
/// For now, comprehensive unit tests (49 tests) provide coverage for the NER functionality.
/// </summary>
public class IngestionPipelineIntegrationTests : IClassFixture<PalimpsestWebApplicationFactory>
{
    private readonly PalimpsestWebApplicationFactory _factory;

    public IngestionPipelineIntegrationTests(PalimpsestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EntityMentionService_SimpleText_DetectsMentions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<PalimpsestDbContext>();
        var mentionService = services.GetRequiredService<IEntityMentionService>();

        var universeId = Guid.NewGuid();
        var universe = new Universe
        {
            UniverseId = universeId,
            Name = "Test Universe",
            Description = "Integration test",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Universes.Add(universe);

        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            VersionId = Guid.NewGuid(),
            Text = "Alice Smith walked into the cafe where Bob was waiting.",
            Ordinal = 0,
            SourceLocator = "{\"start\": 0}",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Segments.Add(segment);
        await dbContext.SaveChangesAsync();

        // Act
        var mentions = await mentionService.DetectMentionsAsync(segment, universeId);

        // Assert
        mentions.Should().NotBeEmpty();
        mentions.Should().Contain(m => m.SurfaceForm == "Alice Smith");
        mentions.Should().Contain(m => m.SurfaceForm == "Bob");
        
        foreach (var mention in mentions)
        {
            mention.UniverseId.Should().Be(universeId);
            mention.SegmentId.Should().Be(segment.SegmentId);
            mention.Confidence.Should().BeGreaterThan(0);
            mention.ResolutionStatus.Should().Be(ResolutionStatus.Unresolved);
        }
    }

    [Fact]
    public async Task EntityResolutionService_NewMention_CreatesEntity()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<PalimpsestDbContext>();
        var resolutionService = services.GetRequiredService<IEntityResolutionService>();
        var entityRepo = services.GetRequiredService<IEntityRepository>();

        var universeId = Guid.NewGuid();
        var universe = new Universe
        {
            UniverseId = universeId,
            Name = "Test Universe 2",
            Description = "Integration test",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Universes.Add(universe);

        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            VersionId = Guid.NewGuid(),
            Text = "Test text",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Segments.Add(segment);
        await dbContext.SaveChangesAsync();

        var mention = new EntityMention
        {
            MentionId = Guid.NewGuid(),
            SegmentId = segment.SegmentId,
            UniverseId = universeId,
            SurfaceForm = "Celestina Maria Foscari",
            SpanStart = 0,
            SpanEnd = 23,
            Confidence = 0.8f,
            ResolutionStatus = ResolutionStatus.Unresolved,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await resolutionService.ResolveMentionAsync(mention);

        // Assert
        mention.EntityId.Should().NotBeNull("mention should be resolved to a new entity");
        
        var entity = await entityRepo.GetByIdAsync(mention.EntityId!.Value);
        entity.Should().NotBeNull();
        entity!.CanonicalName.Should().Be("Celestina Maria Foscari");
        entity.UniverseId.Should().Be(universeId);
        entity.EntityType.Should().Be(EntityType.Person);
    }

    [Fact]
    public async Task EntityResolutionService_WithExactAlias_ResolvesToEntity()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<PalimpsestDbContext>();
        var resolutionService = services.GetRequiredService<IEntityResolutionService>();

        var universeId = Guid.NewGuid();
        var universe = new Universe
        {
            UniverseId = universeId,
            Name = "Test Universe 3",
            Description = "Integration test",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Universes.Add(universe);

        // Create existing entity with alias
        var entity = new Entity
        {
            EntityId = Guid.NewGuid(),
            UniverseId = universeId,
            CanonicalName = "Celestina Maria Foscari",
            EntityType = EntityType.Person,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Entities.Add(entity);

        var alias = new EntityAlias
        {
            AliasId = Guid.NewGuid(),
            EntityId = entity.EntityId,
            Alias = "Celeste",
            AliasNorm = "celeste",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.EntityAliases.Add(alias);

        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            VersionId = Guid.NewGuid(),
            Text = "Test text",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Segments.Add(segment);
        await dbContext.SaveChangesAsync();

        var mention = new EntityMention
        {
            MentionId = Guid.NewGuid(),
            SegmentId = segment.SegmentId,
            UniverseId = universeId,
            SurfaceForm = "Celeste",
            SpanStart = 0,
            SpanEnd = 7,
            Confidence = 0.7f,
            ResolutionStatus = ResolutionStatus.Unresolved,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await resolutionService.ResolveMentionAsync(mention);

        // Assert
        mention.EntityId.Should().Be(entity.EntityId, "mention should resolve to existing entity via alias");
        mention.ResolutionStatus.Should().Be(ResolutionStatus.Resolved);
        mention.Confidence.Should().Be(1.0f, "exact match should have perfect confidence");
    }

    [Fact]
    public async Task EntityResolutionService_BatchProcessing_ResolvesAll()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<PalimpsestDbContext>();
        var resolutionService = services.GetRequiredService<IEntityResolutionService>();

        var universeId = Guid.NewGuid();
        var universe = new Universe
        {
            UniverseId = universeId,
            Name = "Test Universe 4",
            Description = "Integration test",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Universes.Add(universe);

        var segment = new Segment
        {
            SegmentId = Guid.NewGuid(),
            VersionId = Guid.NewGuid(),
            Text = "Test text",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Segments.Add(segment);
        await dbContext.SaveChangesAsync();

        var mentions = new[]
        {
            new EntityMention
            {
                MentionId = Guid.NewGuid(),
                SegmentId = segment.SegmentId,
                UniverseId = universeId,
                SurfaceForm = "Alice",
                SpanStart = 0,
                SpanEnd = 5,
                Confidence = 0.7f,
                ResolutionStatus = ResolutionStatus.Unresolved,
                CreatedAt = DateTime.UtcNow
            },
            new EntityMention
            {
                MentionId = Guid.NewGuid(),
                SegmentId = segment.SegmentId,
                UniverseId = universeId,
                SurfaceForm = "Bob",
                SpanStart = 10,
                SpanEnd = 13,
                Confidence = 0.7f,
                ResolutionStatus = ResolutionStatus.Unresolved,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Act
        await resolutionService.ResolveMentionsBatchAsync(mentions);

        // Assert
        mentions.Should().OnlyContain(m => m.EntityId != null, "all mentions should be resolved");
        mentions.Should().OnlyContain(m => m.ResolutionStatus == ResolutionStatus.Resolved);
        
        // Should create two separate entities
        var entityIds = mentions.Select(m => m.EntityId).Distinct().ToList();
        entityIds.Should().HaveCount(2);
    }
}
