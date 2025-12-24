namespace Palimpsest.Domain.Entities;

/// <summary>
/// Segment represents a chunk of text (chapter/section/paragraph-level unit).
/// Each segment has stable source locators for citation purposes.
/// </summary>
public class Segment
{
    /// <summary>
    /// Gets or sets the unique identifier for the segment.
    /// </summary>
    public Guid SegmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the document version this segment belongs to.
    /// </summary>
    public Guid VersionId { get; set; }
    
    /// <summary>
    /// Gets or sets the optional chapter label for this segment.
    /// </summary>
    public string? ChapterLabel { get; set; }
    
    /// <summary>
    /// Gets or sets the optional hierarchical section path for this segment.
    /// </summary>
    public string? SectionPath { get; set; }
    
    /// <summary>
    /// Gets or sets the ordinal position of this segment within its version.
    /// </summary>
    public int Ordinal { get; set; }
    
    /// <summary>
    /// Gets or sets the text content of this segment.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the source locator information as JSON stored as string.
    /// </summary>
    public string SourceLocator { get; set; } = "{}";
    
    /// <summary>
    /// Gets or sets the timestamp when this segment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the document version this segment belongs to.
    /// </summary>
    public DocumentVersion Version { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the embedding vector for this segment.
    /// </summary>
    public SegmentEmbedding? Embedding { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of entity mentions detected in this segment.
    /// </summary>
    public ICollection<EntityMention> Mentions { get; set; } = new List<EntityMention>();
    
    /// <summary>
    /// Gets or sets the collection of assertions extracted from this segment.
    /// </summary>
    public ICollection<Assertion> Assertions { get; set; } = new List<Assertion>();
}
