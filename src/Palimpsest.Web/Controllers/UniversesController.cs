using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;

namespace Palimpsest.Web.Controllers;

public class UniversesController : Controller
{
    private readonly IUniverseRepository _universeRepository;
    private readonly IUniverseContextService _universeContext;
    private readonly ILogger<UniversesController> _logger;

    public UniversesController(
        IUniverseRepository universeRepository,
        IUniverseContextService universeContext,
        ILogger<UniversesController> logger)
    {
        _universeRepository = universeRepository;
        _universeContext = universeContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var universes = await _universeRepository.GetAllAsync();
        var activeUniverseId = _universeContext.GetActiveUniverseId();
        
        ViewBag.ActiveUniverseId = activeUniverseId;
        return View(universes);
    }

    public IActionResult Create()
    {
        return View();
    }

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

    [HttpPost]
    public IActionResult SetActive(Guid id)
    {
        _universeContext.SetActiveUniverseId(id);
        _logger.LogInformation("Set active universe to {UniverseId}", id);
        return RedirectToAction(nameof(Index));
    }

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
