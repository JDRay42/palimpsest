using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;

namespace Palimpsest.Web.Controllers;

/// <summary>
/// Controller for managing questionable items (conflicts, ambiguities, etc.).
/// </summary>
public class QuestionableController : Controller
{
    private readonly IUniverseContextService _universeContext;
    private readonly ILogger<QuestionableController> _logger;

    public QuestionableController(
        IUniverseContextService universeContext,
        ILogger<QuestionableController> logger)
    {
        _universeContext = universeContext;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of questionable items for the active universe.
    /// </summary>
    public IActionResult Index(Guid? universeId)
    {
        if (universeId.HasValue)
        {
            _universeContext.SetActiveUniverseId(universeId.Value);
        }
        
        var activeUniverseId = _universeContext.RequireActiveUniverseId();
        
        // TODO: Implement questionable items repository
        return View(new List<object>());
    }
}
