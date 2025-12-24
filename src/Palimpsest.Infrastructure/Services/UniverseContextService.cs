using System.Text;
using Microsoft.AspNetCore.Http;
using Palimpsest.Application.Interfaces.Services;

namespace Palimpsest.Infrastructure.Services;

/// <summary>
/// Service for managing active universe context using HTTP session/cookies.
/// Universe acts as the global mode - all operations are scoped to the active universe.
/// </summary>
public class UniverseContextService : IUniverseContextService
{
    private const string UniverseIdSessionKey = "ActiveUniverseId";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UniverseContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetActiveUniverseId()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return null;

        if (!session.TryGetValue(UniverseIdSessionKey, out var bytes))
            return null;

        var universeIdString = Encoding.UTF8.GetString(bytes);
        return Guid.TryParse(universeIdString, out var universeId) ? universeId : null;
    }

    public void SetActiveUniverseId(Guid universeId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            throw new InvalidOperationException("HTTP session is not available");

        var bytes = Encoding.UTF8.GetBytes(universeId.ToString());
        session.Set(UniverseIdSessionKey, bytes);
    }

    public void ClearActiveUniverse()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(UniverseIdSessionKey);
    }

    public Guid RequireActiveUniverseId()
    {
        var universeId = GetActiveUniverseId();
        if (!universeId.HasValue)
        {
            throw new InvalidOperationException("No active universe selected. Please select a universe first.");
        }
        return universeId.Value;
    }
}
