namespace Palimpsest.Domain.Entities;

/// <summary>
/// EntityAlias stores alternative names or references for an entity.
/// Used for entity resolution during mention detection.
/// </summary>
public class EntityAlias
{
    public Guid AliasId { get; set; }
    public Guid EntityId { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string AliasNorm { get; set; } = string.Empty;
    public float Confidence { get; set; } = 0.8f;
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Entity Entity { get; set; } = null!;
}
