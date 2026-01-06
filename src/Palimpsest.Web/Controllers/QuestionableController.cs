using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Web.Controllers;

/// <summary>
/// Controller for managing questionable items (conflicts, ambiguities, etc.).
/// </summary>
public class QuestionableController : Controller
{
    private readonly IUniverseContextService _universeContext;
    private readonly IQuestionableItemRepository _questionableItemRepository;
    private readonly IEntityMentionRepository _entityMentionRepository;
    private readonly ILogger<QuestionableController> _logger;

    public QuestionableController(
        IUniverseContextService universeContext,
        IQuestionableItemRepository questionableItemRepository,
        IEntityMentionRepository entityMentionRepository,
        ILogger<QuestionableController> logger)
    {
        _universeContext = universeContext;
        _questionableItemRepository = questionableItemRepository;
        _entityMentionRepository = entityMentionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of questionable items for the active universe.
    /// </summary>
    public async Task<IActionResult> Index(Guid? universeId, string? status = null)
    {
        if (universeId.HasValue)
        {
            _universeContext.SetActiveUniverseId(universeId.Value);
        }
        
        var activeUniverseId = _universeContext.RequireActiveUniverseId();
        
        // Load questionable items
        var items = await _questionableItemRepository.GetByUniverseIdAsync(activeUniverseId);
        
        // Filter by status if provided
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<QuestionableItemStatus>(status, true, out var statusEnum))
        {
            items = items.Where(i => i.Status == statusEnum).ToList();
            ViewBag.FilteredStatus = statusEnum;
        }
        
        // Group by status for summary
        ViewBag.PendingCount = items.Count(i => i.Status == QuestionableItemStatus.Open);
        ViewBag.ResolvedCount = items.Count(i => i.Status == QuestionableItemStatus.Resolved);
        ViewBag.DismissedCount = items.Count(i => i.Status == QuestionableItemStatus.Dismissed);
        
        return View(items);
    }

    /// <summary>
    /// Shows details for a specific questionable item with resolution options.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _questionableItemRepository.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        // Load associated mention if it exists
        if (item.ObjectKind == ObjectKind.EntityMention && item.ObjectId.HasValue)
        {
            var mention = await _entityMentionRepository.GetByIdAsync(item.ObjectId.Value);
            ViewBag.Mention = mention;
        }

        return View(item);
    }

    /// <summary>
    /// Resolves a questionable item by linking a mention to a selected entity.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id, Guid selectedEntityId, string? notes = null)
    {
        var item = await _questionableItemRepository.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        try
        {
            // Update the questionable item status
            item.Status = QuestionableItemStatus.Resolved;
            item.ResolutionStatus = ResolutionStatus.Resolved;
            item.ResolutionNotes = notes;
            item.ResolvedAt = DateTime.UtcNow;
            await _questionableItemRepository.UpdateAsync(item);

            // If this is an entity mention, update the mention
            if (item.ObjectKind == ObjectKind.EntityMention && item.ObjectId.HasValue)
            {
                var mention = await _entityMentionRepository.GetByIdAsync(item.ObjectId.Value);
                if (mention != null)
                {
                    mention.EntityId = selectedEntityId;
                    mention.ResolutionStatus = ResolutionStatus.Resolved;
                    await _entityMentionRepository.UpdateAsync(mention);
                }
            }

            TempData["SuccessMessage"] = "Questionable item resolved successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving questionable item {ItemId}", id);
            TempData["ErrorMessage"] = "Failed to resolve item: " + ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Dismisses a questionable item as a false positive or not requiring action.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dismiss(Guid id, string? notes = null)
    {
        var item = await _questionableItemRepository.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        try
        {
            // Update the questionable item status
            item.Status = QuestionableItemStatus.Dismissed;
            item.ResolutionNotes = notes;
            item.ResolvedAt = DateTime.UtcNow;
            await _questionableItemRepository.UpdateAsync(item);

            TempData["SuccessMessage"] = "Questionable item dismissed successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing questionable item {ItemId}", id);
            TempData["ErrorMessage"] = "Failed to dismiss item: " + ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Creates a new entity and resolves the questionable item by linking to it.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAndResolve(Guid id, string canonicalName, string entityType, string? aliases = null)
    {
        var item = await _questionableItemRepository.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        // Redirect to the Entities/Create action with pre-filled data
        TempData["ReturnToQuestionable"] = id;
        TempData["SuggestedName"] = canonicalName;
        TempData["SuggestedType"] = entityType;
        TempData["SuggestedAliases"] = aliases;
        
        return RedirectToAction("Create", "Entities");
