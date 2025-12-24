using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// QuestionableItem represents conflicts, identity ambiguities, constraint violations,
/// or low-confidence assertions that require author review and resolution.
/// </summary>
public class QuestionableItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the questionable item.
    /// </summary>
    public Guid ItemId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this item belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the type of questionable item (Conflict, Identity, Constraint, or LowConfidence).
    /// </summary>
    public QuestionableItemType ItemType { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of this item in the review queue.
    /// </summary>
    public QuestionableItemStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the severity level of this item.
    /// </summary>
    public Severity Severity { get; set; }
    
    /// <summary>
    /// Gets or sets the optional identifier of the primary subject entity involved.
    /// </summary>
    public Guid? SubjectEntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the optional identifier of the primary assertion involved.
    /// </summary>
    public Guid? AssertionId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifiers of related assertions as JSON array stored as string.
    /// </summary>
    public string RelatedAssertionIds { get; set; } = "[]";
    
    /// <summary>
    /// Gets or sets the details about this questionable item as JSON stored as string.
    /// </summary>
    public string Details { get; set; } = "{}";
    
    /// <summary>
    /// Gets or sets the timestamp when this item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this item was resolved, if applicable.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the resolution information as JSON stored as string.
    /// </summary>
    public string? Resolution { get; set; }
    
    /// <summary>
    /// Gets or sets the universe this item belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the subject entity if applicable.
    /// </summary>
    public Entity? SubjectEntity { get; set; }
}
