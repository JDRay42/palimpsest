using FluentAssertions;
using Moq;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Palimpsest.Infrastructure.Services;

namespace Palimpsest.Tests.Unit.Services;

public class EntityResolutionServiceTests
{
    private readonly Mock<IEntityRepository> _mockEntityRepository;
    private readonly Mock<IEntityAliasRepository> _mockAliasRepository;
    private readonly Mock<IEntityMentionRepository> _mockMentionRepository;
    private readonly Mock<IQuestionableItemRepository> _mockQuestionableRepository;
    private readonly EntityResolutionService _service;
    private readonly Guid _testUniverseId;
    private readonly Guid _testSegmentId;

    public EntityResolutionServiceTests()
    {
        _mockEntityRepository = new Mock<IEntityRepository>();
        _mockAliasRepository = new Mock<IEntityAliasRepository>();
        _mockMentionRepository = new Mock<IEntityMentionRepository>();
        _mockQuestionableRepository = new Mock<IQuestionableItemRepository>();
        
        _service = new EntityResolutionService(
            _mockEntityRepository.Object,
            _mockAliasRepository.Object,
            _mockMentionRepository.Object,
            _mockQuestionableRepository.Object);

        _testUniverseId = Guid.NewGuid();
        _testSegmentId = Guid.NewGuid();
    }

    [Fact]
    public async Task ResolveMentionAsync_NoMatches_CreatesNewEntity()
    {
        // Arrange
        var mention = CreateTestMention("Alice");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());
        
        _mockAliasRepository
            .Setup(r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());

        Entity? capturedEntity = null;
        _mockEntityRepository
            .Setup(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .Callback<Entity, CancellationToken>((e, ct) => capturedEntity = e)
            .ReturnsAsync((Entity e, CancellationToken ct) => e);

        _mockAliasRepository
            .Setup(r => r.CreateAsync(It.IsAny<EntityAlias>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityAlias a, CancellationToken ct) => a);

        _mockMentionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EntityMention>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ResolveMentionAsync(mention);

        // Assert
        result.EntityId.Should().NotBeNull();
        result.ResolutionStatus.Should().Be(ResolutionStatus.Resolved);
        
        capturedEntity.Should().NotBeNull();
        capturedEntity!.CanonicalName.Should().Be("Alice");
        capturedEntity.EntityType.Should().Be(EntityType.Person);
        
        _mockEntityRepository.Verify(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAliasRepository.Verify(r => r.CreateAsync(It.IsAny<EntityAlias>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveMentionAsync_SingleHighConfidenceMatch_ResolvesAutomatically()
    {
        // Arrange
        var mention = CreateTestMention("Alice");
        var existingEntity = CreateTestEntity("Alice Smith");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)> { (existingEntity, 1.0f) });

        _mockMentionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EntityMention>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ResolveMentionAsync(mention);

        // Assert
        result.EntityId.Should().Be(existingEntity.EntityId);
        result.ResolutionStatus.Should().Be(ResolutionStatus.Resolved);
        
        _mockEntityRepository.Verify(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockQuestionableRepository.Verify(r => r.CreateAsync(It.IsAny<QuestionableItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveMentionAsync_MultipleSimilarCandidates_CreatesQuestionableItem()
    {
        // Arrange
        var mention = CreateTestMention("Alice");
        var entity1 = CreateTestEntity("Alice Smith");
        var entity2 = CreateTestEntity("Alice Johnson");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());
        
        _mockAliasRepository
            .Setup(r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>
            {
                (entity1, 0.85f),
                (entity2, 0.82f)
            });

        _mockMentionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EntityMention>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        QuestionableItem? capturedItem = null;
        _mockQuestionableRepository
            .Setup(r => r.CreateAsync(It.IsAny<QuestionableItem>(), It.IsAny<CancellationToken>()))
            .Callback<QuestionableItem, CancellationToken>((item, ct) => capturedItem = item)
            .ReturnsAsync((QuestionableItem item, CancellationToken ct) => item);

        // Act
        var result = await _service.ResolveMentionAsync(mention);

        // Assert
        result.ResolutionStatus.Should().Be(ResolutionStatus.Candidate);
        result.EntityId.Should().BeNull(); // Not resolved to specific entity
        
        capturedItem.Should().NotBeNull();
        capturedItem!.ItemType.Should().Be(QuestionableItemType.Identity);
        capturedItem.Status.Should().Be(QuestionableItemStatus.Open);
        capturedItem.Severity.Should().Be(Severity.Warn);
        capturedItem.Details.Should().Contain("Alice");
        
        _mockQuestionableRepository.Verify(r => r.CreateAsync(It.IsAny<QuestionableItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEntityRepository.Verify(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveMentionAsync_SingleMediumConfidenceCandidate_ResolvesWithAdjustedConfidence()
    {
        // Arrange
        var mention = CreateTestMention("Alice", 0.9f);
        var existingEntity = CreateTestEntity("Alice Smith");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());
        
        _mockAliasRepository
            .Setup(r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>
            {
                (existingEntity, 0.80f)
            });

        _mockMentionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EntityMention>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ResolveMentionAsync(mention);

        // Assert
        result.EntityId.Should().Be(existingEntity.EntityId);
        result.ResolutionStatus.Should().Be(ResolutionStatus.Resolved);
        result.Confidence.Should().Be(0.80f); // Adjusted to match candidate confidence
        
        _mockQuestionableRepository.Verify(r => r.CreateAsync(It.IsAny<QuestionableItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveMentionAsync_BelowThreshold_LeavesUnresolved()
    {
        // Arrange
        var mention = CreateTestMention("Alice");
        var existingEntity = CreateTestEntity("Alicia Smith");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());
        
        _mockAliasRepository
            .Setup(r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>
            {
                (existingEntity, 0.60f) // Below 0.75 threshold
            });

        _mockMentionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EntityMention>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ResolveMentionAsync(mention);

        // Assert
        result.ResolutionStatus.Should().Be(ResolutionStatus.Unresolved);
        result.EntityId.Should().BeNull();
        
        _mockEntityRepository.Verify(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockQuestionableRepository.Verify(r => r.CreateAsync(It.IsAny<QuestionableItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveMentionsBatchAsync_MultipleMentions_ResolvesAll()
    {
        // Arrange
        var mentions = new[]
        {
            CreateTestMention("Alice"),
            CreateTestMention("Bob")
        };
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());
        
        _mockAliasRepository
            .Setup(r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());

        _mockEntityRepository
            .Setup(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entity e, CancellationToken ct) => e);

        _mockAliasRepository
            .Setup(r => r.CreateAsync(It.IsAny<EntityAlias>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityAlias a, CancellationToken ct) => a);

        _mockMentionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EntityMention>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var results = await _service.ResolveMentionsBatchAsync(mentions);

        // Assert
        var resultList = results.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().AllSatisfy(m => m.ResolutionStatus.Should().Be(ResolutionStatus.Resolved));
        resultList.Should().AllSatisfy(m => m.EntityId.Should().NotBeNull());
        
        _mockEntityRepository.Verify(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task FindCandidateEntitiesAsync_ExactMatchExists_ReturnsExactMatch()
    {
        // Arrange
        var entity = CreateTestEntity("Alice Smith");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)> { (entity, 1.0f) });

        // Act
        var result = await _service.FindCandidateEntitiesAsync(_testUniverseId, "Alice");

        // Assert
        var candidates = result.ToList();
        candidates.Should().ContainSingle();
        candidates[0].Entity.Should().Be(entity);
        candidates[0].Confidence.Should().Be(1.0f);
        
        // Should not call fuzzy match if exact match found
        _mockAliasRepository.Verify(
            r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FindCandidateEntitiesAsync_NoExactMatch_CallsFuzzyMatch()
    {
        // Arrange
        var entity = CreateTestEntity("Alicia Smith");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());
        
        _mockAliasRepository
            .Setup(r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)> { (entity, 0.85f) });

        // Act
        var result = await _service.FindCandidateEntitiesAsync(_testUniverseId, "Alice");

        // Assert
        var candidates = result.ToList();
        candidates.Should().ContainSingle();
        candidates[0].Confidence.Should().Be(0.85f);
        
        _mockAliasRepository.Verify(
            r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, 0.75f, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolveMentionAsync_OrganizationName_InfersOrgType()
    {
        // Arrange
        var mention = CreateTestMention("Acme Corp");
        
        _mockAliasRepository
            .Setup(r => r.FindExactMatchAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());
        
        _mockAliasRepository
            .Setup(r => r.FindSimilarMatchesAsync(It.IsAny<string>(), _testUniverseId, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Entity, float)>());

        Entity? capturedEntity = null;
        _mockEntityRepository
            .Setup(r => r.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .Callback<Entity, CancellationToken>((e, ct) => capturedEntity = e)
            .ReturnsAsync((Entity e, CancellationToken ct) => e);

        _mockAliasRepository
            .Setup(r => r.CreateAsync(It.IsAny<EntityAlias>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityAlias a, CancellationToken ct) => a);

        _mockMentionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EntityMention>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ResolveMentionAsync(mention);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.EntityType.Should().Be(EntityType.Org);
    }

    private EntityMention CreateTestMention(string surfaceForm, float confidence = 0.8f)
    {
        return new EntityMention
        {
            MentionId = Guid.NewGuid(),
            UniverseId = _testUniverseId,
            SegmentId = _testSegmentId,
            SurfaceForm = surfaceForm,
            SpanStart = 0,
            SpanEnd = surfaceForm.Length,
            Confidence = confidence,
            ResolutionStatus = ResolutionStatus.Unresolved,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Entity CreateTestEntity(string canonicalName)
    {
        return new Entity
        {
            EntityId = Guid.NewGuid(),
            UniverseId = _testUniverseId,
            CanonicalName = canonicalName,
            EntityType = EntityType.Person,
            CreatedAt = DateTime.UtcNow
        };
    }
}
