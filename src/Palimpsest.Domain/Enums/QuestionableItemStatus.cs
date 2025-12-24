namespace Palimpsest.Domain.Enums;

/// <summary>
/// Status of questionable items in the review queue.
/// </summary>
public enum QuestionableItemStatus
{
    /// <summary>
    /// Item is open and requires attention.
    /// </summary>
    Open,
    
    /// <summary>
    /// Item has been dismissed by the author.
    /// </summary>
    Dismissed,
    
    /// <summary>
    /// Item has been resolved by the author.
    /// </summary>
    Resolved
}
