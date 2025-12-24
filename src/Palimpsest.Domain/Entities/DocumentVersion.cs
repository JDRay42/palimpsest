namespace Palimpsest.Domain.Entities;

/// <summary>
/// DocumentVersion captures a snapshot of a document's content.
/// Supports versioning for re-ingestion and change tracking.
/// </summary>
public class DocumentVersion
{
    /// <summary>
    /// Gets or sets the unique identifier for the document version.
    /// </summary>
    public Guid VersionId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the document this version belongs to.
    /// </summary>
    public Guid DocumentId { get; set; }
    
    /// <summary>
    /// Gets or sets the hash of the ingested content for change detection.
    /// </summary>
    public string IngestHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the raw text content as originally provided.
    /// </summary>
    public string RawText { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the normalized text content after preprocessing.
    /// </summary>
    public string NormalizedText { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when this version was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the document this version belongs to.
    /// </summary>
    public Document Document { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the collection of segments in this version.
    /// </summary>
    public ICollection<Segment> Segments { get; set; } = new List<Segment>();
}
