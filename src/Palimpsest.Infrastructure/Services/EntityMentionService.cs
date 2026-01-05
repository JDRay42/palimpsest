using System.Text.RegularExpressions;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Infrastructure.Services;

/// <summary>
/// Service for detecting entity mentions in text segments.
/// Uses capitalization patterns and other heuristics to identify potential named entities.
/// </summary>
public class EntityMentionService : IEntityMentionService
{
    // Common words to exclude from entity detection
    private static readonly HashSet<string> CommonWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "The", "A", "An", "This", "That", "These", "Those",
        "I", "You", "He", "She", "It", "We", "They",
        "My", "Your", "His", "Her", "Its", "Our", "Their",
        "Me", "Him", "Us", "Them",
        "What", "When", "Where", "Why", "How", "Who",
        "Which", "Whose", "Whom",
        "But", "Or", "And", "Nor", "For", "Yet", "So",
        "At", "In", "On", "By", "To", "From", "With",
        "Mr", "Mrs", "Ms", "Dr", "Prof", "Rev",
        "Chapter", "Part", "Section", "Book", "Volume"
    };

    // Regex for detecting capitalized sequences
    private static readonly Regex CapitalizedSequenceRegex = new(
        @"\b([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\b",
        RegexOptions.Compiled);

    // Regex for detecting all-caps sequences (potential acronyms)
    private static readonly Regex AllCapsRegex = new(
        @"\b([A-Z]{2,})\b",
        RegexOptions.Compiled);

    // Regex for possessive forms
    private static readonly Regex PossessiveRegex = new(
        @"'s\b",
        RegexOptions.Compiled);

    public Task<IEnumerable<EntityMention>> DetectMentionsAsync(
        Segment segment,
        Guid universeId,
        CancellationToken cancellationToken = default)
    {
        var mentions = new List<EntityMention>();
        var text = segment.Text;

        // Skip empty or very short segments
        if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
        {
            return Task.FromResult<IEnumerable<EntityMention>>(mentions);
        }

        // Detect capitalized sequences
        var capitalizedMatches = CapitalizedSequenceRegex.Matches(text);
        foreach (Match match in capitalizedMatches)
        {
            var surfaceForm = match.Value;
            
            // Skip if it's a common word or at start of sentence
            if (IsCommonWord(surfaceForm) || IsLikelySentenceStart(text, match.Index))
            {
                continue;
            }

            // Calculate confidence based on various factors
            var confidence = CalculateConfidence(surfaceForm, text, match.Index);

            mentions.Add(new EntityMention
            {
                MentionId = Guid.NewGuid(),
                UniverseId = universeId,
                EntityId = null, // Unresolved initially
                SegmentId = segment.SegmentId,
                SurfaceForm = surfaceForm,
                SpanStart = match.Index,
                SpanEnd = match.Index + match.Length,
                Confidence = confidence,
                ResolutionStatus = ResolutionStatus.Unresolved,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Detect all-caps sequences (acronyms, organizations)
        var allCapsMatches = AllCapsRegex.Matches(text);
        foreach (Match match in allCapsMatches)
        {
            var surfaceForm = match.Value;
            
            // Skip very short acronyms unless they appear multiple times
            if (surfaceForm.Length < 3)
            {
                continue;
            }

            // Higher confidence for acronyms
            var confidence = 0.8f;

            mentions.Add(new EntityMention
            {
                MentionId = Guid.NewGuid(),
                UniverseId = universeId,
                EntityId = null,
                SegmentId = segment.SegmentId,
                SurfaceForm = surfaceForm,
                SpanStart = match.Index,
                SpanEnd = match.Index + match.Length,
                Confidence = confidence,
                ResolutionStatus = ResolutionStatus.Unresolved,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Remove duplicates (prefer higher confidence)
        mentions = mentions
            .GroupBy(m => new { m.SpanStart, m.SpanEnd })
            .Select(g => g.OrderByDescending(m => m.Confidence).First())
            .OrderBy(m => m.SpanStart)
            .ToList();

        return Task.FromResult<IEnumerable<EntityMention>>(mentions);
    }

    public async Task<IEnumerable<EntityMention>> DetectMentionsBatchAsync(
        IEnumerable<Segment> segments,
        Guid universeId,
        CancellationToken cancellationToken = default)
    {
        var allMentions = new List<EntityMention>();

        foreach (var segment in segments)
        {
            var mentions = await DetectMentionsAsync(segment, universeId, cancellationToken);
            allMentions.AddRange(mentions);
        }

        return allMentions;
    }

    private static bool IsCommonWord(string word)
    {
        // Split multi-word sequences and check each word
        var words = word.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // If all words are common, exclude
        if (words.All(w => CommonWords.Contains(w)))
        {
            return true;
        }

        // Special case: single common word
        if (words.Length == 1 && CommonWords.Contains(word))
        {
            return true;
        }

        return false;
    }

    private static bool IsLikelySentenceStart(string text, int position)
    {
        // Check if this is at the beginning of the text
        if (position < 3)
        {
            return true;
        }

        // Look backwards for sentence-ending punctuation
        var precedingText = text.Substring(Math.Max(0, position - 3), Math.Min(3, position));
        
        // If preceded by ". ", "! ", "? ", or newline, likely sentence start
        return precedingText.Contains(". ") ||
               precedingText.Contains("! ") ||
               precedingText.Contains("? ") ||
               precedingText.Contains("\n");
    }

    private static float CalculateConfidence(string surfaceForm, string text, int position)
    {
        var confidence = 0.6f; // Base confidence

        // Increase confidence for multi-word names
        var wordCount = surfaceForm.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount >= 2)
        {
            confidence += 0.2f;
        }

        // Increase confidence if not at sentence start
        if (!IsLikelySentenceStart(text, position))
        {
            confidence += 0.1f;
        }

        // Increase confidence if appears with possessive
        var endPos = position + surfaceForm.Length;
        if (endPos < text.Length - 2)
        {
            var following = text.Substring(endPos, Math.Min(2, text.Length - endPos));
            if (following.StartsWith("'s") || following.StartsWith("'s"))
            {
                confidence += 0.1f;
            }
        }

        // Cap at 1.0
        return Math.Min(confidence, 1.0f);
    }
}
