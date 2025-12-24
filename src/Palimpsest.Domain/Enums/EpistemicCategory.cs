namespace Palimpsest.Domain.Enums;

/// <summary>
/// Epistemic category for assertions as defined in the specification.
/// </summary>
public enum EpistemicCategory
{
    /// <summary>
    /// Narrator-level or otherwise asserted as factual by the text (as extracted).
    /// </summary>
    Observed,
    
    /// <summary>
    /// A character believes/says it; may be wrong.
    /// </summary>
    Believed,
    
    /// <summary>
    /// Derived by the tool (explicitly tracked).
    /// </summary>
    Inferred
}
