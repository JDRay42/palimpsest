using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;

namespace Palimpsest.Web.Controllers;

/// <summary>
/// Controller for managing entities within the active universe.
/// </summary>
public class EntitiesController : Controller
{
    private readonly IEntityRepository _entityRepository;
    private readonly IUniverseContextService _universeContext;
    private readonly ILogger<EntitiesController> _logger;

    public EntitiesController(
        IEntityRepository entityRepository,
        IUniverseContextService universeContext,
        ILogger<EntitiesController> logger)
    {
        _entityRepository = entityRepository;
        _universeContext = universeContext;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of entities for the active universe.
    /// </summary>
    public async Task<IActionResult> Index(Guid? universeId)
    {
        if (universeId.HasValue)
        {
            _universeContext.SetActiveUniverseId(universeId.Value);
        }
        
        var activeUniverseId = _universeContext.RequireActiveUniverseId();
        var entities = await _entityRepository.GetByUniverseIdAsync(activeUniverseId);
        return View(entities);
    }

    /// <summary>
    /// Displays the details of a specific entity.
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        var entity = await _entityRepository.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        return View(entity);
    }

    /// <summary>
    /// Search entities by name.
    /// </summary>
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return RedirectToAction(nameof(Index));
        }

        var universeId = _universeContext.RequireActiveUniverseId();
        var results = await _entityRepository.SearchByNameAsync(universeId, query);
        
        ViewBag.SearchQuery = query;
        return View("Index", results);
    }
}
