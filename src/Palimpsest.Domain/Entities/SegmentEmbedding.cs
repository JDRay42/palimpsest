namespace Palimpsest.Domain.Entities;

/// <summary>
/// SegmentEmbedding stores the vector embedding for semantic search.
/// One-to-one relationship with Segment.
/// </summary>
public class SegmentEmbedding
{
    /// <summary>
    /// Gets or sets the segment identifier this embedding belongs to.
    /// </summary>
    public Guid SegmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the embedding vector array for semantic search.
    /// </summary>
    public float[]? Embedding { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the model used to generate this embedding.
    /// </summary>
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when this embedding was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the segment this embedding belongs to.
    /// </summary>
    public Segment Segment { get; set; } = null!;
}
