using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Entity represents a person, place, organization, object, concept, or event-like thing
/// within a universe. This is a node in the knowledge graph.
/// </summary>
public class Entity
{
    public Guid EntityId { get; set; }
    public Guid UniverseId { get; set; }
    public EntityType EntityType { get; set; }
    public string CanonicalName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Universe Universe { get; set; } = null!;
    public ICollection<EntityAlias> Aliases { get; set; } = new List<EntityAlias>();
    public ICollection<EntityMention> Mentions { get; set; } = new List<EntityMention>();
    public ICollection<Assertion> SubjectAssertions { get; set; } = new List<Assertion>();
    public ICollection<Assertion> ObjectAssertions { get; set; } = new List<Assertion>();
    public ICollection<Edge> EdgesFrom { get; set; } = new List<Edge>();
    public ICollection<Edge> EdgesTo { get; set; } = new List<Edge>();
    public Dossier? Dossier { get; set; }
}
