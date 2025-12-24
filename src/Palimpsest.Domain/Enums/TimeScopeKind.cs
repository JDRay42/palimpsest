namespace Palimpsest.Domain.Enums;

/// <summary>
/// Time scope discriminator for assertions.
/// </summary>
public enum TimeScopeKind
{
    /// <summary>
    /// Time scope is not known or not applicable.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Assertion applies to an exact date.
    /// </summary>
    Exact,
    
    /// <summary>
    /// Assertion applies to a date range.
    /// </summary>
    Range
}
