namespace Palimpsest.Domain.Enums;

/// <summary>
/// Type discriminator for assertion objects (typed union pattern).
/// </summary>
public enum ObjectKind
{
    /// <summary>
    /// Object references another entity.
    /// </summary>
    Entity,
    
    /// <summary>
    /// Object is a literal value (string, number, date, etc.).
    /// </summary>
    Literal,
    
    /// <summary>
    /// Object is structured JSON data.
    /// </summary>
    Json
}
