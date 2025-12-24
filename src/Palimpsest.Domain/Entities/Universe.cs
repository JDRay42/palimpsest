using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Universe is the top-level boundary for all canon operations.
/// All entities, assertions, and documents are scoped to a single universe.
/// </summary>
public class Universe
{
    public Guid UniverseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Entity> Entities { get; set; } = new List<Entity>();
    public ICollection<EntityMention> EntityMentions { get; set; } = new List<EntityMention>();
    public ICollection<Assertion> Assertions { get; set; } = new List<Assertion>();
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
    public ICollection<QuestionableItem> QuestionableItems { get; set; } = new List<QuestionableItem>();
    public ICollection<Dossier> Dossiers { get; set; } = new List<Dossier>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
