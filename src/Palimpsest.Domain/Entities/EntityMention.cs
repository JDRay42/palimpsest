using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// EntityMention represents a detected entity reference in a segment.
/// May be resolved to an entity, have candidate entities, or be unresolved.
/// </summary>
public class EntityMention
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity mention.
    /// </summary>
    public Guid MentionId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this mention belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the resolved entity, if resolved.
    /// </summary>
    public Guid? EntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the segment containing this mention.
    /// </summary>
    public Guid SegmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the text form of the entity as it appears in the segment.
    /// </summary>
    public string SurfaceForm { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the starting character position of the mention in the segment.
    /// </summary>
    public int SpanStart { get; set; }
    
    /// <summary>
    /// Gets or sets the ending character position of the mention in the segment.
    /// </summary>
    public int SpanEnd { get; set; }
    
    /// <summary>
    /// Gets or sets the confidence score for this mention detection (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; set; }
    
    /// <summary>
    /// Gets or sets the resolution status indicating whether this mention has been linked to an entity.
    /// </summary>
    public ResolutionStatus ResolutionStatus { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this mention was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the universe this mention belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the resolved entity, if this mention has been linked.
    /// </summary>
    public Entity? Entity { get; set; }
    
    /// <summary>
    /// Gets or sets the segment containing this mention.
    /// </summary>
    public Segment Segment { get; set; } = null!;
}
