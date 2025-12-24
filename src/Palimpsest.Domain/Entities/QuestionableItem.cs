using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// QuestionableItem represents conflicts, identity ambiguities, constraint violations,
/// or low-confidence assertions that require author review and resolution.
/// </summary>
public class QuestionableItem
{
    public Guid ItemId { get; set; }
    public Guid UniverseId { get; set; }
    public QuestionableItemType ItemType { get; set; }
    public QuestionableItemStatus Status { get; set; }
    public Severity Severity { get; set; }
    public Guid? SubjectEntityId { get; set; }
    public Guid? AssertionId { get; set; }
    public string RelatedAssertionIds { get; set; } = "[]"; // JSON array stored as string
    public string Details { get; set; } = "{}"; // JSON stored as string
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; } // JSON stored as string
    
    // Navigation properties
    public Universe Universe { get; set; } = null!;
    public Entity? SubjectEntity { get; set; }
}
