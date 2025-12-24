namespace Palimpsest.Domain.Entities;

/// <summary>
/// SegmentEmbedding stores the vector embedding for semantic search.
/// One-to-one relationship with Segment.
/// </summary>
public class SegmentEmbedding
{
    public Guid SegmentId { get; set; }
    // Using float[] for now; will be mapped to pgvector in Infrastructure
    public float[]? Embedding { get; set; }
    public string Model { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Segment Segment { get; set; } = null!;
}
