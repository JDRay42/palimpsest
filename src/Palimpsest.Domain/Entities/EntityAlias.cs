namespace Palimpsest.Domain.Entities;

/// <summary>
/// EntityAlias stores alternative names or references for an entity.
/// Used for entity resolution during mention detection.
/// </summary>
public class EntityAlias
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity alias.
    /// </summary>
    public Guid AliasId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the entity this alias belongs to.
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the alternative name or reference for the entity.
    /// </summary>
    public string Alias { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the normalized form of the alias for matching.
    /// </summary>
    public string AliasNorm { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the confidence score for this alias (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; set; } = 0.8f;
    
    /// <summary>
    /// Gets or sets the timestamp when this alias was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the entity this alias belongs to.
    /// </summary>
    public Entity Entity { get; set; } = null!;
}
