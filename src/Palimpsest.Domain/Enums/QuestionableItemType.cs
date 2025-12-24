namespace Palimpsest.Domain.Enums;

/// <summary>
/// Types of questionable items requiring author attention.
/// </summary>
public enum QuestionableItemType
{
    /// <summary>
    /// Conflicting assertions detected.
    /// </summary>
    Conflict,
    
    /// <summary>
    /// Identity ambiguity for entity mentions.
    /// </summary>
    Identity,
    
    /// <summary>
    /// Constraint violation detected.
    /// </summary>
    Constraint,
    
    /// <summary>
    /// Low confidence assertion flagged for review.
    /// </summary>
    LowConfidence
}
