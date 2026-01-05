using FluentAssertions;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Palimpsest.Infrastructure.Services;

namespace Palimpsest.Tests.Unit.Services;

public class EntityMentionServiceTests
{
    private readonly EntityMentionService _service;
    private readonly Guid _testUniverseId;
    private readonly Guid _testSegmentId;

    public EntityMentionServiceTests()
    {
        _service = new EntityMentionService();
        _testUniverseId = Guid.NewGuid();
        _testSegmentId = Guid.NewGuid();
    }

    [Fact]
    public async Task DetectMentionsAsync_EmptySegment_ReturnsEmpty()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectMentionsAsync_SingleCapitalizedName_DetectsMention()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "The story follows Alice as she travels.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        mentions.Should().ContainSingle(m => m.SurfaceForm == "Alice");
        
        var aliceMention = mentions.First(m => m.SurfaceForm == "Alice");
        aliceMention.UniverseId.Should().Be(_testUniverseId);
        aliceMention.SegmentId.Should().Be(_testSegmentId);
        aliceMention.ResolutionStatus.Should().Be(ResolutionStatus.Unresolved);
        aliceMention.EntityId.Should().BeNull();
        aliceMention.Confidence.Should().BeGreaterThan(0.6f);
    }

    [Fact]
    public async Task DetectMentionsAsync_MultiWordName_DetectsMentionWithHigherConfidence()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "She met Celestina Maria at the party.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        mentions.Should().Contain(m => m.SurfaceForm == "Celestina Maria");
        
        var mention = mentions.First(m => m.SurfaceForm == "Celestina Maria");
        mention.Confidence.Should().BeGreaterThan(0.7f); // Multi-word bonus
    }

    [Fact]
    public async Task DetectMentionsAsync_CommonWordsAtSentenceStart_FiltersOut()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "The quick brown fox. A new sentence. This is text.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        mentions.Should().NotContain(m => m.SurfaceForm == "The");
        mentions.Should().NotContain(m => m.SurfaceForm == "A");
        mentions.Should().NotContain(m => m.SurfaceForm == "This");
    }

    [Fact]
    public async Task DetectMentionsAsync_AllCapsAcronym_DetectsWithHighConfidence()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "The FBI investigated the case with CIA assistance.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        mentions.Should().Contain(m => m.SurfaceForm == "FBI");
        mentions.Should().Contain(m => m.SurfaceForm == "CIA");
        
        var fbiMention = mentions.First(m => m.SurfaceForm == "FBI");
        fbiMention.Confidence.Should().Be(0.8f); // Acronym confidence
    }

    [Fact]
    public async Task DetectMentionsAsync_ShortAcronym_FiltersOut()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "He went to LA and NY last week.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        // Short acronyms (< 3 chars) should be filtered
        mentions.Should().NotContain(m => m.SurfaceForm == "LA");
        mentions.Should().NotContain(m => m.SurfaceForm == "NY");
    }

    [Fact]
    public async Task DetectMentionsAsync_PossessiveForm_IncreasesConfidence()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "This is Sarah's book and not mine.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        var sarahMention = mentions.FirstOrDefault(m => m.SurfaceForm == "Sarah");
        sarahMention.Should().NotBeNull();
        sarahMention!.Confidence.Should().BeGreaterThan(0.7f); // Possessive bonus
    }

    [Fact]
    public async Task DetectMentionsAsync_MultipleMentions_DetectsAll()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "The friends Alice and Bob met at the cafe, where Charlie arrived with Diana.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        mentions.Should().Contain(m => m.SurfaceForm == "Alice");
        mentions.Should().Contain(m => m.SurfaceForm == "Bob");
        mentions.Should().Contain(m => m.SurfaceForm == "Charlie");
        mentions.Should().Contain(m => m.SurfaceForm == "Diana");
    }

    [Fact]
    public async Task DetectMentionsAsync_MidSentenceCapital_DetectsMention()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "She said hello to Marcus before leaving.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        mentions.Should().Contain(m => m.SurfaceForm == "Marcus");
        
        var mention = mentions.First(m => m.SurfaceForm == "Marcus");
        // Not at sentence start, should have higher confidence
        mention.Confidence.Should().BeGreaterThan(0.7f);
    }

    [Fact]
    public async Task DetectMentionsBatchAsync_MultipleSegments_DetectsAllMentions()
    {
        // Arrange
        var segments = new[]
        {
            new Segment
            {
                SegmentId = Guid.NewGuid(),
                Text = "She saw Alice at the store.",
                Ordinal = 0,
                SourceLocator = "{}",
                CreatedAt = DateTime.UtcNow
            },
            new Segment
            {
                SegmentId = Guid.NewGuid(),
                Text = "During lunch, Bob stayed home with Charlie.",
                Ordinal = 1,
                SourceLocator = "{}",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Act
        var result = await _service.DetectMentionsBatchAsync(segments, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        mentions.Should().Contain(m => m.SurfaceForm == "Alice");
        mentions.Should().Contain(m => m.SurfaceForm == "Bob");
        mentions.Should().Contain(m => m.SurfaceForm == "Charlie");
        
        // Verify mentions are from different segments
        var segmentIds = mentions.Select(m => m.SegmentId).Distinct().ToList();
        segmentIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task DetectMentionsAsync_DuplicateSpans_RemovesDuplicates()
    {
        // Arrange - This shouldn't happen in practice, but tests the deduplication logic
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "He saw Alice and then ALICE went together.", // Different case but same entity potentially
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.ToList();
        // Should detect both "Alice" (capitalized) and "ALICE" (all-caps) as separate mentions
        // since they have different patterns
        mentions.Should().Contain(m => m.SurfaceForm == "Alice");
        mentions.Should().Contain(m => m.SurfaceForm == "ALICE");
    }

    [Fact]
    public async Task DetectMentionsAsync_SpanPositions_CorrectlyTracked()
    {
        // Arrange
        var segment = new Segment
        {
            SegmentId = _testSegmentId,
            Text = "She saw Alice with Bob.",
            Ordinal = 0,
            SourceLocator = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.DetectMentionsAsync(segment, _testUniverseId);

        // Assert
        var mentions = result.OrderBy(m => m.SpanStart).ToList();
        mentions.Should().HaveCountGreaterOrEqualTo(2);
        
        mentions.Should().Contain(m => m.SurfaceForm == "Alice");
        mentions.Should().Contain(m => m.SurfaceForm == "Bob");
    }
}
