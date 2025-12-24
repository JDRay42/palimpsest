using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Universe is the top-level boundary for all canon operations.
/// All entities, assertions, and documents are scoped to a single universe.
/// </summary>
public class Universe
{
    /// <summary>
    /// Gets or sets the unique identifier for the universe.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the universe.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the optional author name associated with this universe.
    /// </summary>
    public string? AuthorName { get; set; }
    
    /// <summary>
    /// Gets or sets the optional description of the universe.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this universe was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of documents in this universe.
    /// </summary>
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    
    /// <summary>
    /// Gets or sets the collection of entities in this universe.
    /// </summary>
    public ICollection<Entity> Entities { get; set; } = new List<Entity>();
    
    /// <summary>
    /// Gets or sets the collection of entity mentions in this universe.
    /// </summary>
    public ICollection<EntityMention> EntityMentions { get; set; } = new List<EntityMention>();
    
    /// <summary>
    /// Gets or sets the collection of assertions in this universe.
    /// </summary>
    public ICollection<Assertion> Assertions { get; set; } = new List<Assertion>();
    
    /// <summary>
    /// Gets or sets the collection of edges (entity-to-entity relationships) in this universe.
    /// </summary>
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
    
    /// <summary>
    /// Gets or sets the collection of questionable items requiring author review in this universe.
    /// </summary>
    public ICollection<QuestionableItem> QuestionableItems { get; set; } = new List<QuestionableItem>();
    
    /// <summary>
    /// Gets or sets the collection of dossiers in this universe.
    /// </summary>
    public ICollection<Dossier> Dossiers { get; set; } = new List<Dossier>();
    
    /// <summary>
    /// Gets or sets the collection of background jobs for this universe.
    /// </summary>
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
