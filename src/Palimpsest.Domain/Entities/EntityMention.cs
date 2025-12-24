using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// EntityMention represents a detected entity reference in a segment.
/// May be resolved to an entity, have candidate entities, or be unresolved.
/// </summary>
public class EntityMention
{
    public Guid MentionId { get; set; }
    public Guid UniverseId { get; set; }
    public Guid? EntityId { get; set; }
    public Guid SegmentId { get; set; }
    public string SurfaceForm { get; set; } = string.Empty;
    public int SpanStart { get; set; }
    public int SpanEnd { get; set; }
    public float Confidence { get; set; }
    public ResolutionStatus ResolutionStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Universe Universe { get; set; } = null!;
    public Entity? Entity { get; set; }
    public Segment Segment { get; set; } = null!;
}
