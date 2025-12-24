namespace Palimpsest.Domain.Entities;

/// <summary>
/// Edge denormalizes entity-to-entity relationships for graph traversal.
/// Created automatically from assertions where ObjectKind = Entity.
/// </summary>
public class Edge
{
    public Guid EdgeId { get; set; }
    public Guid UniverseId { get; set; }
    public Guid FromEntityId { get; set; }
    public Guid ToEntityId { get; set; }
    public string Relation { get; set; } = string.Empty;
    public Guid AssertionId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Universe Universe { get; set; } = null!;
    public Entity FromEntity { get; set; } = null!;
    public Entity ToEntity { get; set; } = null!;
    public Assertion Assertion { get; set; } = null!;
}
