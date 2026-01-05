using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Infrastructure.Services;

/// <summary>
/// Service for document ingestion pipeline.
/// Handles text normalization, segmentation, entity mention detection, and entity resolution.
/// Phase 2: Entity mention detection and resolution integrated.
/// </summary>
public class IngestionService : IIngestionService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ISegmentRepository _segmentRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IEntityMentionService _entityMentionService;
    private readonly IEntityResolutionService _entityResolutionService;

    public IngestionService(
        IDocumentRepository documentRepository,
        ISegmentRepository segmentRepository,
        IJobRepository jobRepository,
        IEmbeddingService embeddingService,
        IEntityMentionService entityMentionService,
        IEntityResolutionService entityResolutionService)
    {
        _documentRepository = documentRepository;
        _segmentRepository = segmentRepository;
        _jobRepository = jobRepository;
        _embeddingService = embeddingService;
        _entityMentionService = entityMentionService;
        _entityResolutionService = entityResolutionService;
    }

    public async Task<Guid> IngestDocumentAsync(
        Guid universeId,
        Guid documentId,
        string rawText,
        CancellationToken cancellationToken = default)
    {
        // Normalize text
        var normalizedText = NormalizeText(rawText);
        
        // Compute ingest hash
        var ingestHash = ComputeHash(rawText + normalizedText);
        
        // Create document version
        var version = new DocumentVersion
        {
            VersionId = Guid.NewGuid(),
            DocumentId = documentId,
            IngestHash = ingestHash,
            RawText = rawText,
            NormalizedText = normalizedText,
            CreatedAt = DateTime.UtcNow
        };
        
        // Note: In a real implementation, we'd save the version through a repository
        // For now, this is simplified for Phase 1
        
        // Segment text
        var segments = await SegmentTextAsync(version.VersionId, normalizedText, cancellationToken);
        
        // Create segments (this would normally be done through the repository)
        await _segmentRepository.CreateRangeAsync(segments, cancellationToken);
        
        // Create ingestion job
        var job = new Job
        {
            JobId = Guid.NewGuid(),
            UniverseId = universeId,
            DocumentId = documentId,
            JobType = JobType.Ingest,
            Status = JobStatus.Running,
            Progress = "{\"stage\": \"segmentation_complete\", \"segments_created\": " + segments.Count() + "}",
            CreatedAt = DateTime.UtcNow
        };
        
        await _jobRepository.CreateAsync(job, cancellationToken);
        
        try
        {
            // Phase 2: Entity mention detection
            job.Progress = "{\"stage\": \"mention_detection_started\", \"segments_created\": " + segments.Count() + "}";
            await _jobRepository.UpdateAsync(job, cancellationToken);
            
            var mentions = await _entityMentionService.DetectMentionsBatchAsync(segments, universeId, cancellationToken);
            var mentionsList = mentions.ToList();
            
            job.Progress = "{\"stage\": \"mention_detection_complete\", \"segments_created\": " + segments.Count() + 
                          ", \"mentions_detected\": " + mentionsList.Count + "}";
            await _jobRepository.UpdateAsync(job, cancellationToken);
            
            // Phase 2: Entity resolution
            job.Progress = "{\"stage\": \"entity_resolution_started\", \"segments_created\": " + segments.Count() + 
                          ", \"mentions_detected\": " + mentionsList.Count + "}";
            await _jobRepository.UpdateAsync(job, cancellationToken);
            
            var resolvedMentions = await _entityResolutionService.ResolveMentionsBatchAsync(mentionsList, cancellationToken);
            var resolvedList = resolvedMentions.ToList();
            
            var resolvedCount = resolvedList.Count(m => m.ResolutionStatus == ResolutionStatus.Resolved);
            var candidateCount = resolvedList.Count(m => m.ResolutionStatus == ResolutionStatus.Candidate);
            var unresolvedCount = resolvedList.Count(m => m.ResolutionStatus == ResolutionStatus.Unresolved);
            
            job.Status = JobStatus.Succeeded;
            job.Progress = "{\"stage\": \"complete\", \"segments_created\": " + segments.Count() + 
                          ", \"mentions_detected\": " + mentionsList.Count + 
                          ", \"mentions_resolved\": " + resolvedCount +
                          ", \"mentions_candidate\": " + candidateCount +
                          ", \"mentions_unresolved\": " + unresolvedCount + "}";
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job, cancellationToken);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.Error = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job, cancellationToken);
            throw;
        }
        
        return job.JobId;
    }

    public Task<IEnumerable<Segment>> SegmentTextAsync(
        Guid versionId,
        string normalizedText,
        CancellationToken cancellationToken = default)
    {
        var segments = new List<Segment>();
        
        // Simple paragraph-based segmentation for Phase 1
        // TODO: Implement more sophisticated chapter/section-aware segmentation
        
        var paragraphs = normalizedText.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        int ordinal = 0;
        int currentOffset = 0;
        
        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
                continue;
            
            var trimmed = paragraph.Trim();
            
            // Try to detect chapter headers
            string? chapterLabel = null;
            if (IsChapterHeader(trimmed))
            {
                chapterLabel = trimmed;
            }
            
            var segment = new Segment
            {
                SegmentId = Guid.NewGuid(),
                VersionId = versionId,
                ChapterLabel = chapterLabel,
                SectionPath = null, // TODO: Build section path from document structure
                Ordinal = ordinal++,
                Text = trimmed,
                SourceLocator = $"{{\"offset\": {currentOffset}, \"length\": {trimmed.Length}}}",
                CreatedAt = DateTime.UtcNow
            };
            
            segments.Add(segment);
            currentOffset += paragraph.Length + 2; // Account for paragraph separator
        }
        
        return Task.FromResult<IEnumerable<Segment>>(segments);
    }

    private static string NormalizeText(string text)
    {
        // Basic normalization for Phase 1
        // TODO: Enhance with more sophisticated normalization
        
        // Normalize line endings
        text = text.Replace("\r\n", "\n");
        
        // Remove excessive whitespace
        text = Regex.Replace(text, @"[ \t]+", " ");
        
        // Normalize multiple newlines
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        
        return text.Trim();
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static bool IsChapterHeader(string text)
    {
        // Simple heuristic for chapter detection
        // TODO: Enhance with more sophisticated patterns
        
        if (text.Length > 100)
            return false;
        
        var lowerText = text.ToLowerInvariant();
        
        return lowerText.StartsWith("chapter ") ||
               lowerText.StartsWith("prologue") ||
               lowerText.StartsWith("epilogue") ||
               Regex.IsMatch(text, @"^(Chapter|Part|Section|Book)\s+\d+", RegexOptions.IgnoreCase);
    }
}
