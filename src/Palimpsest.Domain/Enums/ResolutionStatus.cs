namespace Palimpsest.Domain.Enums;

/// <summary>
/// Resolution status for entity mentions.
/// </summary>
public enum ResolutionStatus
{
    /// <summary>
    /// Mention has been successfully linked to an entity.
    /// </summary>
    Resolved,
    
    /// <summary>
    /// Mention has candidate entities but not yet resolved.
    /// </summary>
    Candidate,
    
    /// <summary>
    /// Mention has not been linked to any entity.
    /// </summary>
    Unresolved
}
