namespace Palimpsest.Domain.Entities;

/// <summary>
/// Segment represents a chunk of text (chapter/section/paragraph-level unit).
/// Each segment has stable source locators for citation purposes.
/// </summary>
public class Segment
{
    public Guid SegmentId { get; set; }
    public Guid VersionId { get; set; }
    public string? ChapterLabel { get; set; }
    public string? SectionPath { get; set; }
    public int Ordinal { get; set; }
    public string Text { get; set; } = string.Empty;
    public string SourceLocator { get; set; } = "{}"; // JSON stored as string
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public DocumentVersion Version { get; set; } = null!;
    public SegmentEmbedding? Embedding { get; set; }
    public ICollection<EntityMention> Mentions { get; set; } = new List<EntityMention>();
    public ICollection<Assertion> Assertions { get; set; } = new List<Assertion>();
}
