namespace Palimpsest.Domain.Entities;

/// <summary>
/// Edge denormalizes entity-to-entity relationships for graph traversal.
/// Created automatically from assertions where ObjectKind = Entity.
/// </summary>
public class Edge
{
    /// <summary>
    /// Gets or sets the unique identifier for the edge.
    /// </summary>
    public Guid EdgeId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this edge belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the entity this edge originates from.
    /// </summary>
    public Guid FromEntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the entity this edge points to.
    /// </summary>
    public Guid ToEntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the relation type or predicate for this edge.
    /// </summary>
    public string Relation { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the identifier of the assertion this edge is derived from.
    /// </summary>
    public Guid AssertionId { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this edge was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the universe this edge belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the entity this edge originates from.
    /// </summary>
    public Entity FromEntity { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the entity this edge points to.
    /// </summary>
    public Entity ToEntity { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the assertion this edge is derived from.
    /// </summary>
    public Assertion Assertion { get; set; } = null!;
}
