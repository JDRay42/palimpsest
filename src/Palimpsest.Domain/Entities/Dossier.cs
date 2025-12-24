namespace Palimpsest.Domain.Entities;

/// <summary>
/// Dossier is a materialized bounded context packet for an entity.
/// Contains curated facts, relationships, and conflicts for LLM grounding.
/// One-to-one with Entity.
/// </summary>
public class Dossier
{
    public Guid EntityId { get; set; }
    public Guid UniverseId { get; set; }
    public string Content { get; set; } = "{}"; // JSON stored as string
    public string ContentText { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Entity Entity { get; set; } = null!;
    public Universe Universe { get; set; } = null!;
}
