namespace Palimpsest.Domain.Enums;

/// <summary>
/// Severity levels for questionable items.
/// </summary>
public enum Severity
{
    /// <summary>
    /// Informational item that may not require immediate action.
    /// </summary>
    Info,
    
    /// <summary>
    /// Warning indicating a potential issue that should be reviewed.
    /// </summary>
    Warn,
    
    /// <summary>
    /// Error indicating a critical issue requiring attention.
    /// </summary>
    Error
}
