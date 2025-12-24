namespace Palimpsest.Domain.Enums;

/// <summary>
/// Entity types as defined in the specification.
/// </summary>
public enum EntityType
{
    /// <summary>
    /// A person or character.
    /// </summary>
    Person,
    
    /// <summary>
    /// A place or location.
    /// </summary>
    Place,
    
    /// <summary>
    /// An organization or group.
    /// </summary>
    Org,
    
    /// <summary>
    /// An object or item.
    /// </summary>
    Object,
    
    /// <summary>
    /// A concept or abstract idea.
    /// </summary>
    Concept,
    
    /// <summary>
    /// An event or time-bound occurrence.
    /// </summary>
    EventLike
}
