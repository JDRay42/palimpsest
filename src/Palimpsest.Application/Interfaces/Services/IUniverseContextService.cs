namespace Palimpsest.Application.Interfaces.Services;

/// <summary>
/// Service for managing the active universe context.
/// Universe acts as the global mode - all operations are scoped to the active universe.
/// </summary>
public interface IUniverseContextService
{
    /// <summary>
    /// Gets the currently active universe ID.
    /// </summary>
    Guid? GetActiveUniverseId();
    
    /// <summary>
    /// Sets the active universe ID.
    /// </summary>
    void SetActiveUniverseId(Guid universeId);
    
    /// <summary>
    /// Clears the active universe selection.
    /// </summary>
    void ClearActiveUniverse();
    
    /// <summary>
    /// Ensures an active universe is set, throws if not.
    /// </summary>
    Guid RequireActiveUniverseId();
}
