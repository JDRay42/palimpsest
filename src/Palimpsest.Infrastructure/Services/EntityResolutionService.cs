using System.Text.Json;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Infrastructure.Services;

/// <summary>
/// Service for resolving entity mentions to canonical entities.
/// Handles exact matches, fuzzy matches, ambiguous cases, and new entity creation.
/// </summary>
public class EntityResolutionService : IEntityResolutionService
{
    private readonly IEntityRepository _entityRepository;
    private readonly IEntityAliasRepository _aliasRepository;
    private readonly IEntityMentionRepository _mentionRepository;
    private readonly IQuestionableItemRepository _questionableItemRepository;

    // Configuration thresholds
    private const float ExactMatchThreshold = 1.0f;
    private const float HighConfidenceThreshold = 0.85f;
    private const float AmbiguityThreshold = 0.75f;
    private const int MaxCandidates = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityResolutionService"/> class.
    /// </summary>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="aliasRepository">The entity alias repository.</param>
    /// <param name="mentionRepository">The entity mention repository.</param>
    /// <param name="questionableItemRepository">The questionable item repository.</param>
    public EntityResolutionService(
        IEntityRepository entityRepository,
        IEntityAliasRepository aliasRepository,
        IEntityMentionRepository mentionRepository,
        IQuestionableItemRepository questionableItemRepository)
    {
        _entityRepository = entityRepository;
        _aliasRepository = aliasRepository;
        _mentionRepository = mentionRepository;
        _questionableItemRepository = questionableItemRepository;
    }

    public async Task<EntityMention> ResolveMentionAsync(
        EntityMention mention,
        CancellationToken cancellationToken = default)
    {
        // Get candidate entities for this mention
        var candidates = await FindCandidateEntitiesAsync(
            mention.UniverseId,
            mention.SurfaceForm,
            cancellationToken);

        var candidateList = candidates.ToList();

        // Case 1: No matches - create new entity
        if (!candidateList.Any())
        {
            var newEntity = await CreateNewEntityAsync(mention, cancellationToken);
            mention.EntityId = newEntity.EntityId;
            mention.ResolutionStatus = ResolutionStatus.Resolved;
            await _mentionRepository.UpdateAsync(mention, cancellationToken);
            return mention;
        }

        // Case 2: Single high-confidence match - resolve to that entity
        if (candidateList.Count == 1 && candidateList[0].Confidence >= HighConfidenceThreshold)
        {
            mention.EntityId = candidateList[0].Entity.EntityId;
            mention.ResolutionStatus = ResolutionStatus.Resolved;
            await _mentionRepository.UpdateAsync(mention, cancellationToken);
            return mention;
        }

        // Case 3: Multiple candidates with similar confidence - ambiguous, create questionable item
        var topCandidates = candidateList
            .Where(c => c.Confidence >= AmbiguityThreshold)
            .OrderByDescending(c => c.Confidence)
            .Take(MaxCandidates)
            .ToList();

        if (topCandidates.Count > 1)
        {
            await CreateAmbiguityQuestionableItemAsync(mention, topCandidates, cancellationToken);
            mention.ResolutionStatus = ResolutionStatus.Candidate;
            await _mentionRepository.UpdateAsync(mention, cancellationToken);
            return mention;
        }

        // Case 4: Single candidate above threshold - resolve with medium confidence
        if (topCandidates.Count == 1)
        {
            mention.EntityId = topCandidates[0].Entity.EntityId;
            mention.ResolutionStatus = ResolutionStatus.Resolved;
            mention.Confidence = Math.Min(mention.Confidence, topCandidates[0].Confidence);
            await _mentionRepository.UpdateAsync(mention, cancellationToken);
            return mention;
        }

        // Case 5: No candidates above threshold - leave unresolved
        mention.ResolutionStatus = ResolutionStatus.Unresolved;
        await _mentionRepository.UpdateAsync(mention, cancellationToken);
        return mention;
    }

    public async Task<IEnumerable<EntityMention>> ResolveMentionsBatchAsync(
        IEnumerable<EntityMention> mentions,
        CancellationToken cancellationToken = default)
    {
        var resolvedMentions = new List<EntityMention>();

        foreach (var mention in mentions)
        {
            var resolved = await ResolveMentionAsync(mention, cancellationToken);
            resolvedMentions.Add(resolved);
        }

        return resolvedMentions;
    }

    public async Task<IEnumerable<(Entity Entity, float Confidence)>> FindCandidateEntitiesAsync(
        Guid universeId,
        string surfaceForm,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeForMatching(surfaceForm);

        // First, try exact match
        var exactMatches = await _aliasRepository.FindExactMatchAsync(
            normalized,
            universeId,
            cancellationToken);

        if (exactMatches.Any())
        {
            return exactMatches;
        }

        // If no exact match, try fuzzy matching
        var fuzzyMatches = await _aliasRepository.FindSimilarMatchesAsync(
            surfaceForm,
            universeId,
            AmbiguityThreshold,
            MaxCandidates,
            cancellationToken);

        return fuzzyMatches;
    }

    /// <summary>
    /// Creates a new entity for an unmatched mention.
    /// </summary>
    private async Task<Entity> CreateNewEntityAsync(
        EntityMention mention,
        CancellationToken cancellationToken)
    {
        // Create new entity with surface form as canonical name
        var entity = new Entity
        {
            EntityId = Guid.NewGuid(),
            UniverseId = mention.UniverseId,
            EntityType = InferEntityType(mention.SurfaceForm),
            CanonicalName = mention.SurfaceForm,
            CreatedAt = DateTime.UtcNow
        };

        entity = await _entityRepository.CreateAsync(entity, cancellationToken);

        // Create initial alias
        var alias = new EntityAlias
        {
            AliasId = Guid.NewGuid(),
            EntityId = entity.EntityId,
            Alias = mention.SurfaceForm,
            AliasNorm = NormalizeForMatching(mention.SurfaceForm),
            Confidence = 1.0f,
            CreatedAt = DateTime.UtcNow
        };

        await _aliasRepository.CreateAsync(alias, cancellationToken);

        return entity;
    }

    /// <summary>
    /// Creates a questionable item for ambiguous entity resolution.
    /// </summary>
    private async Task CreateAmbiguityQuestionableItemAsync(
        EntityMention mention,
        List<(Entity Entity, float Confidence)> candidates,
        CancellationToken cancellationToken)
    {
        var details = new
        {
            mention_id = mention.MentionId,
            surface_form = mention.SurfaceForm,
            segment_id = mention.SegmentId,
            candidates = candidates.Select(c => new
            {
                entity_id = c.Entity.EntityId,
                canonical_name = c.Entity.CanonicalName,
                entity_type = c.Entity.EntityType.ToString(),
                confidence = c.Confidence
            }).ToList()
        };

        var questionableItem = new QuestionableItem
        {
            ItemId = Guid.NewGuid(),
            UniverseId = mention.UniverseId,
            ItemType = QuestionableItemType.Identity,
            Status = QuestionableItemStatus.Open,
            Severity = Severity.Warn,
            SubjectEntityId = candidates.FirstOrDefault().Entity?.EntityId,
            AssertionId = null,
            RelatedAssertionIds = "[]",
            Details = JsonSerializer.Serialize(details),
            CreatedAt = DateTime.UtcNow
        };

        await _questionableItemRepository.CreateAsync(questionableItem, cancellationToken);
    }

    /// <summary>
    /// Normalizes text for alias matching (lowercase, trim).
    /// </summary>
    private static string NormalizeForMatching(string text)
    {
        return text.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Infers entity type from surface form heuristics.
    /// This is a simple heuristic; V2 could use ML classification.
    /// </summary>
    private static EntityType InferEntityType(string surfaceForm)
    {
        // Simple heuristics
        if (surfaceForm.Contains("Corp") || surfaceForm.Contains("Inc") || 
            surfaceForm.Contains("LLC") || surfaceForm.Contains("Ltd"))
        {
            return EntityType.Org;
        }

        // All caps might be organization or place acronym
        if (surfaceForm.All(c => char.IsUpper(c) || char.IsWhiteSpace(c)))
        {
            return EntityType.Org;
        }

        // Multiple words with "the" might be place
        if (surfaceForm.StartsWith("The ") && surfaceForm.Split(' ').Length >= 2)
        {
            return EntityType.Place;
        }

        // Default to Person for single capitalized names
        return EntityType.Person;
    }
}
