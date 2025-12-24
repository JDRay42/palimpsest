using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Entity represents a person, place, organization, object, concept, or event-like thing
/// within a universe. This is a node in the knowledge graph.
/// </summary>
public class Entity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this entity belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the type classification of the entity.
    /// </summary>
    public EntityType EntityType { get; set; }
    
    /// <summary>
    /// Gets or sets the canonical name used to identify this entity.
    /// </summary>
    public string CanonicalName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the universe this entity belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the collection of alternative names for this entity.
    /// </summary>
    public ICollection<EntityAlias> Aliases { get; set; } = new List<EntityAlias>();
    
    /// <summary>
    /// Gets or sets the collection of mentions of this entity in text segments.
    /// </summary>
    public ICollection<EntityMention> Mentions { get; set; } = new List<EntityMention>();
    
    /// <summary>
    /// Gets or sets the collection of assertions where this entity is the subject.
    /// </summary>
    public ICollection<Assertion> SubjectAssertions { get; set; } = new List<Assertion>();
    
    /// <summary>
    /// Gets or sets the collection of assertions where this entity is the object.
    /// </summary>
    public ICollection<Assertion> ObjectAssertions { get; set; } = new List<Assertion>();
    
    /// <summary>
    /// Gets or sets the collection of edges originating from this entity.
    /// </summary>
    public ICollection<Edge> EdgesFrom { get; set; } = new List<Edge>();
    
    /// <summary>
    /// Gets or sets the collection of edges pointing to this entity.
    /// </summary>
    public ICollection<Edge> EdgesTo { get; set; } = new List<Edge>();
    
    /// <summary>
    /// Gets or sets the dossier containing curated facts about this entity.
    /// </summary>
    public Dossier? Dossier { get; set; }
}
