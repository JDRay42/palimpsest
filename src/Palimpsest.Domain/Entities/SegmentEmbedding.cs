namespace Palimpsest.Domain.Entities;

/// <summary>
/// SegmentEmbedding stores the vector embedding for semantic search.
/// One-to-one relationship with Segment.
/// </summary>
public class SegmentEmbedding
{
    public Guid SegmentId { get; set; }
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public string Model { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Segment Segment { get; set; } = null!;
}
