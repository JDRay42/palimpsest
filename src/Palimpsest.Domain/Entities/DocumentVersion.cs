namespace Palimpsest.Domain.Entities;

/// <summary>
/// DocumentVersion captures a snapshot of a document's content.
/// Supports versioning for re-ingestion and change tracking.
/// </summary>
public class DocumentVersion
{
    public Guid VersionId { get; set; }
    public Guid DocumentId { get; set; }
    public string IngestHash { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public string NormalizedText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Document Document { get; set; } = null!;
    public ICollection<Segment> Segments { get; set; } = new List<Segment>();
}
