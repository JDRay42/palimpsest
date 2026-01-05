using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;

namespace Palimpsest.Web.Controllers;

/// <summary>
/// Controller for managing assertions (facts) within the active universe.
/// </summary>
public class AssertionsController : Controller
{
    private readonly IAssertionRepository _assertionRepository;
    private readonly IUniverseContextService _universeContext;
    private readonly ILogger<AssertionsController> _logger;

    public AssertionsController(
        IAssertionRepository assertionRepository,
        IUniverseContextService universeContext,
        ILogger<AssertionsController> logger)
    {
        _assertionRepository = assertionRepository;
        _universeContext = universeContext;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of assertions for the active universe.
    /// </summary>
    public async Task<IActionResult> Index(Guid? universeId)
    {
        if (universeId.HasValue)
        {
            _universeContext.SetActiveUniverseId(universeId.Value);
        }
        
        var activeUniverseId = _universeContext.RequireActiveUniverseId();
        var assertions = await _assertionRepository.GetByUniverseIdAsync(activeUniverseId);
        return View(assertions);
    }

    /// <summary>
    /// Displays the details of a specific assertion.
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        var assertion = await _assertionRepository.GetByIdAsync(id);
        if (assertion == null)
        {
            return NotFound();
        }

        return View(assertion);
    }
}
