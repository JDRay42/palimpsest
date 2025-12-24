namespace Palimpsest.Domain.Entities;

/// <summary>
/// Dossier is a materialized bounded context packet for an entity.
/// Contains curated facts, relationships, and conflicts for LLM grounding.
/// One-to-one with Entity.
/// </summary>
public class Dossier
{
    /// <summary>
    /// Gets or sets the entity identifier this dossier belongs to.
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this dossier belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the curated content as JSON stored as string.
    /// </summary>
    public string Content { get; set; } = "{}";
    
    /// <summary>
    /// Gets or sets the text representation of the content for search and display.
    /// </summary>
    public string ContentText { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when this dossier was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the entity this dossier belongs to.
    /// </summary>
    public Entity Entity { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the universe this dossier belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
}
