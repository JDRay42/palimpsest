using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;

namespace Palimpsest.Web.Controllers;

/// <summary>
/// Controller for managing universes in the Palimpsest application.
/// </summary>
public class UniversesController : Controller
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IUniverseContextService _universeContext;
    private readonly ILogger<UniversesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UniversesController"/> class.
    /// </summary>
    /// <param name="universeRepository">The universe repository.</param>
    /// <param name="universeContext">The universe context service.</param>
    /// <param name="logger">The logger.</param>
    public UniversesController(
        IUniverseRepository universeRepository,
        IUniverseContextService universeContext,
        ILogger<UniversesController> logger)
    {
        _universeRepository = universeRepository;
        _universeContext = universeContext;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of all universes.
    /// </summary>
    /// <returns>The view displaying all universes.</returns>
    public async Task<IActionResult> Index()
    {
        var universes = await _universeRepository.GetAllAsync();
        var activeUniverseId = _universeContext.GetActiveUniverseId();
        
        ViewBag.ActiveUniverseId = activeUniverseId;
        return View(universes);
    }

    /// <summary>
    /// Displays the form for creating a new universe.
    /// </summary>
    /// <returns>The view for creating a universe.</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Creates a new universe.
    /// </summary>
    /// <param name="universe">The universe to create.</param>
    /// <returns>Redirects to the index page on success; returns the form on validation errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Universe universe)
    {
        if (ModelState.IsValid)
        {
            universe.UniverseId = Guid.NewGuid();
            await _universeRepository.CreateAsync(universe);
            _logger.LogInformation("Created universe {UniverseId} - {Name}", universe.UniverseId, universe.Name);
            
            // Automatically set as active universe
            _universeContext.SetActiveUniverseId(universe.UniverseId);
            
            return RedirectToAction(nameof(Index));
        }
        return View(universe);
    }

    /// <summary>
    /// Sets a universe as the active universe.
    /// </summary>
    /// <param name="id">The unique identifier of the universe to activate.</param>
    /// <returns>Redirects to the index page.</returns>
    [HttpPost]
    public IActionResult SetActive(Guid id)
    {
        _universeContext.SetActiveUniverseId(id);
        _logger.LogInformation("Set active universe to {UniverseId}", id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Displays the details of a specific universe.
    /// </summary>
    /// <param name="id">The unique identifier of the universe.</param>
    /// <returns>The view displaying universe details, or NotFound if the universe doesn't exist.</returns>
    public async Task<IActionResult> Details(Guid id)
    {
        var universe = await _universeRepository.GetByIdAsync(id);
        if (universe == null)
        {
            return NotFound();
        }
        return View(universe);
    }
}
