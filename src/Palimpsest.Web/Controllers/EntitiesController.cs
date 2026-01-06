using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using System.Text.Json;

namespace Palimpsest.Web.Controllers;

/// <summary>
/// Controller for managing entities within the active universe.
/// </summary>
public class EntitiesController : Controller
{
    private readonly IEntityRepository _entityRepository;
    private readonly IEntityAliasRepository _entityAliasRepository;
    private readonly IEntityMentionRepository _entityMentionRepository;
    private readonly IUniverseContextService _universeContext;
    private readonly ILogger<EntitiesController> _logger;

    public EntitiesController(
        IEntityRepository entityRepository,
        IEntityAliasRepository entityAliasRepository,
        IEntityMentionRepository entityMentionRepository,
        IUniverseContextService universeContext,
        ILogger<EntitiesController> logger)
    {
        _entityRepository = entityRepository;
        _entityAliasRepository = entityAliasRepository;
        _entityMentionRepository = entityMentionRepository;
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

        // Load mentions for this entity
        var mentions = await _entityMentionRepository.GetByEntityIdAsync(id);
        ViewBag.Mentions = mentions.OrderByDescending(m => m.CreatedAt).ToList();

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

    /// <summary>
    /// Displays the form to create a new entity.
    /// </summary>
    public IActionResult Create()
    {
        var universeId = _universeContext.RequireActiveUniverseId();
        ViewBag.UniverseId = universeId;
        return View();
    }

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Entity entity, string? aliasesText)
    {
        try
        {
            var universeId = _universeContext.RequireActiveUniverseId();
            entity.UniverseId = universeId;
            entity.EntityId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(entity.CanonicalName))
            {
                ModelState.AddModelError("CanonicalName", "Canonical name is required");
                return View(entity);
            }

            await _entityRepository.CreateAsync(entity);

            // Add canonical name as primary alias
            var primaryAlias = new EntityAlias
            {
                AliasId = Guid.NewGuid(),
                EntityId = entity.EntityId,
                Alias = entity.CanonicalName,
                AliasNorm = entity.CanonicalName.Trim().ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow
            };
            await _entityAliasRepository.CreateAsync(primaryAlias);

            // Parse and add additional aliases (comma or newline separated)
            if (!string.IsNullOrWhiteSpace(aliasesText))
            {
                var aliasLines = aliasesText.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var aliasNames = aliasLines
                    .Select(line => line.Trim())
                    .Where(aliasName => !string.IsNullOrWhiteSpace(aliasName) && 
                                       !aliasName.Equals(entity.CanonicalName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                foreach (var aliasName in aliasNames)
                {
                    var alias = new EntityAlias
                    {
                        AliasId = Guid.NewGuid(),
                        EntityId = entity.EntityId,
                        Alias = aliasName,
                        AliasNorm = aliasName.ToLowerInvariant(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _entityAliasRepository.CreateAsync(alias);
                }
            }

            TempData["SuccessMessage"] = $"Entity '{entity.CanonicalName}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = entity.EntityId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity");
            ModelState.AddModelError("", "An error occurred while creating the entity.");
            return View(entity);
        }
    }

    /// <summary>
    /// Displays the form to edit an existing entity.
    /// </summary>
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _entityRepository.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        // Load aliases
        var aliases = await _entityAliasRepository.GetByEntityIdAsync(id);
        ViewBag.Aliases = aliases.Where(a => !a.Alias.Equals(entity.CanonicalName, StringComparison.OrdinalIgnoreCase))
                                   .Select(a => a.Alias)
                                   .ToList();

        return View(entity);
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Entity entity, string? aliasesText)
    {
        if (id != entity.EntityId)
        {
            return BadRequest();
        }

        try
        {
            var existing = await _entityRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Update entity fields
            existing.CanonicalName = entity.CanonicalName;
            existing.EntityType = entity.EntityType;
            
            await _entityRepository.UpdateAsync(existing);

            // Update aliases - remove old, add new
            var existingAliases = await _entityAliasRepository.GetByEntityIdAsync(id);
            
            // Keep canonical name alias
            var canonicalAlias = existingAliases.FirstOrDefault(a => 
                a.Alias.Equals(entity.CanonicalName, StringComparison.OrdinalIgnoreCase));
            
            if (canonicalAlias == null)
            {
                canonicalAlias = new EntityAlias
                {
                    AliasId = Guid.NewGuid(),
                    EntityId = entity.EntityId,
                    Alias = entity.CanonicalName,
                    AliasNorm = entity.CanonicalName.Trim().ToLowerInvariant(),
                    CreatedAt = DateTime.UtcNow
                };
                await _entityAliasRepository.CreateAsync(canonicalAlias);
            }
            else if (canonicalAlias.Alias != entity.CanonicalName)
            {
                canonicalAlias.Alias = entity.CanonicalName;
                canonicalAlias.AliasNorm = entity.CanonicalName.Trim().ToLowerInvariant();
                await _entityAliasRepository.UpdateAsync(canonicalAlias);
            }

            // Parse new aliases
            var newAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(aliasesText))
            {
                var aliasLines = aliasesText.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var validAliases = aliasLines
                    .Select(line => line.Trim())
                    .Where(trimmed => !string.IsNullOrWhiteSpace(trimmed));
                
                foreach (var trimmed in validAliases)
                {
                    newAliases.Add(trimmed);
                }
            }

            // Remove aliases not in new list (except canonical)
            var aliasesToRemove = existingAliases
                .Where(alias => !alias.Alias.Equals(entity.CanonicalName, StringComparison.OrdinalIgnoreCase) &&
                               !newAliases.Contains(alias.Alias))
                .ToList();
            
            foreach (var alias in aliasesToRemove)
            {
                await _entityAliasRepository.DeleteAsync(alias.AliasId);
            }

            // Add new aliases
            var existingAliasNames = new HashSet<string>(
                existingAliases.Select(a => a.Alias), 
                StringComparer.OrdinalIgnoreCase);
            
            var aliasesToAdd = newAliases
                .Where(aliasName => !existingAliasNames.Contains(aliasName) && 
                                   !aliasName.Equals(entity.CanonicalName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            foreach (var aliasName in aliasesToAdd)
            {
                var alias = new EntityAlias
                {
                    AliasId = Guid.NewGuid(),
                    EntityId = entity.EntityId,
                    Alias = aliasName,
                    AliasNorm = aliasName.ToLowerInvariant(),
                    CreatedAt = DateTime.UtcNow
                };
                await _entityAliasRepository.CreateAsync(alias);
            }

            TempData["SuccessMessage"] = $"Entity '{entity.CanonicalName}' updated successfully.";
            return RedirectToAction(nameof(Details), new { id = entity.EntityId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity {EntityId}", id);
            ModelState.AddModelError("", "An error occurred while updating the entity.");
            return View(entity);
        }
    }

    /// <summary>
    /// Displays confirmation page for deleting an entity.
    /// </summary>
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _entityRepository.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        // Check for dependencies
        var mentions = await _entityMentionRepository.GetByEntityIdAsync(id);
        ViewBag.MentionCount = mentions.Count();

        return View(entity);
    }

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            var entity = await _entityRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            // Delete aliases first (cascading should handle this, but be explicit)
            var aliases = await _entityAliasRepository.GetByEntityIdAsync(id);
            foreach (var alias in aliases)
            {
                await _entityAliasRepository.DeleteAsync(alias.AliasId);
            }

            // Unlink mentions (set entity_id to null rather than deleting mentions)
            var mentions = await _entityMentionRepository.GetByEntityIdAsync(id);
            foreach (var mention in mentions)
            {
                mention.EntityId = null;
                mention.ResolutionStatus = ResolutionStatus.Unresolved;
                await _entityMentionRepository.UpdateAsync(mention);
            }

            await _entityRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = $"Entity '{entity.CanonicalName}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity {EntityId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the entity.";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    /// <summary>
    /// Displays the bulk import form.
    /// </summary>
    public IActionResult Import()
    {
        var universeId = _universeContext.RequireActiveUniverseId();
        ViewBag.UniverseId = universeId;
        return View();
    }

    /// <summary>
    /// Imports entities from JSON or CSV format.
    /// Format: [{"name": "Character Name", "type": "Person", "aliases": ["Alias1", "Alias2"]}]
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file, string format)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Please upload a file");
            return View();
        }

        try
        {
            var universeId = _universeContext.RequireActiveUniverseId();
            var imported = 0;
            var skipped = 0;

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            if (format == "json")
            {
                var entities = JsonSerializer.Deserialize<List<ImportEntityDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (entities != null)
                {
                    foreach (var dto in entities)
                    {
                        if (string.IsNullOrWhiteSpace(dto.Name))
                        {
                            skipped++;
                            continue;
                        }

                        // Check if entity already exists
                        var existing = (await _entityRepository.SearchByNameAsync(universeId, dto.Name))
                            .FirstOrDefault(e => e.CanonicalName.Equals(dto.Name, StringComparison.OrdinalIgnoreCase));

                        if (existing != null)
                        {
                            _logger.LogInformation("Skipping duplicate entity: {Name}", dto.Name);
                            skipped++;
                            continue;
                        }

                        var entity = new Entity
                        {
                            EntityId = Guid.NewGuid(),
                            UniverseId = universeId,
                            CanonicalName = dto.Name,
                            EntityType = ParseEntityType(dto.Type),
                            CreatedAt = DateTime.UtcNow
                        };

                        await _entityRepository.CreateAsync(entity);

                        // Add canonical name as alias
                        var primaryAlias = new EntityAlias
                        {
                            AliasId = Guid.NewGuid(),
                            EntityId = entity.EntityId,
                            Alias = entity.CanonicalName,
                            AliasNorm = entity.CanonicalName.Trim().ToLowerInvariant(),
                            CreatedAt = DateTime.UtcNow
                        };
                        await _entityAliasRepository.CreateAsync(primaryAlias);

                        // Add additional aliases
                        if (dto.Aliases != null)
                        {
                            var validAliases = dto.Aliases
                                .Where(aliasName => !string.IsNullOrWhiteSpace(aliasName) && 
                                                   !aliasName.Equals(entity.CanonicalName, StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            
                            foreach (var aliasName in validAliases)
                            {
                                var alias = new EntityAlias
                                {
                                    AliasId = Guid.NewGuid(),
                                    EntityId = entity.EntityId,
                                    Alias = aliasName,
                                    AliasNorm = aliasName.ToLowerInvariant(),
                                    CreatedAt = DateTime.UtcNow
                                };
                                await _entityAliasRepository.CreateAsync(alias);
                            }
                        }

                        imported++;
                    }
                }
            }
            else if (format == "csv")
            {
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var hasHeader = lines.Length > 0 && 
                    (lines[0].Contains("name", StringComparison.OrdinalIgnoreCase) || 
                     lines[0].Contains("character", StringComparison.OrdinalIgnoreCase));

                var startIndex = hasHeader ? 1 : 0;

                for (int i = startIndex; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
                    {
                        skipped++;
                        continue;
                    }

                    var name = parts[0].Trim().Trim('"');
                    var type = parts.Length > 1 ? parts[1].Trim().Trim('"') : "Person";
                    var aliases = parts.Length > 2 ? parts[2].Split(';').Select(a => a.Trim().Trim('"')).ToList() : new List<string>();

                    // Check for duplicates
                    var existing = (await _entityRepository.SearchByNameAsync(universeId, name))
                        .FirstOrDefault(e => e.CanonicalName.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        skipped++;
                        continue;
                    }

                    var entity = new Entity
                    {
                        EntityId = Guid.NewGuid(),
                        UniverseId = universeId,
                        CanonicalName = name,
                        EntityType = ParseEntityType(type),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _entityRepository.CreateAsync(entity);

                    // Add canonical alias
                    var primaryAlias = new EntityAlias
                    {
                        AliasId = Guid.NewGuid(),
                        EntityId = entity.EntityId,
                        Alias = entity.CanonicalName,
                        AliasNorm = entity.CanonicalName.Trim().ToLowerInvariant(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _entityAliasRepository.CreateAsync(primaryAlias);

                    // Add aliases
                    var validAliases = aliases
                        .Where(aliasName => !string.IsNullOrWhiteSpace(aliasName) && 
                                           !aliasName.Equals(entity.CanonicalName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    
                    foreach (var aliasName in validAliases)
                    {
                        var alias = new EntityAlias
                        {
                            AliasId = Guid.NewGuid(),
                            EntityId = entity.EntityId,
                            Alias = aliasName,
                            AliasNorm = aliasName.ToLowerInvariant(),
                            CreatedAt = DateTime.UtcNow
                        };
                        await _entityAliasRepository.CreateAsync(alias);
                    }

                    imported++;
                }
            }
            else
            {
                ModelState.AddModelError("format", "Invalid format specified");
                return View();
            }

            TempData["SuccessMessage"] = $"Imported {imported} entities. Skipped {skipped} duplicates.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing entities");
            ModelState.AddModelError("", $"Error importing entities: {ex.Message}");
            return View();
        }
    }

    /// <summary>
    /// Updates the resolution of an entity mention.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateMentionResolution(Guid mentionId, Guid? entityId, string action)
    {
        try
        {
            var mention = await _entityMentionRepository.GetByIdAsync(mentionId);
            if (mention == null)
            {
                return Json(new { success = false, message = "Mention not found" });
            }

            switch (action)
            {
                case "approve":
                    // Approve current resolution
                    mention.ResolutionStatus = ResolutionStatus.Resolved;
                    mention.Confidence = 1.0f;
                    break;

                case "reject":
                    // Reject and mark as unresolved
                    mention.EntityId = null;
                    mention.ResolutionStatus = ResolutionStatus.Unresolved;
                    break;

                case "remap":
                    // Remap to different entity
                    if (entityId.HasValue)
                    {
                        mention.EntityId = entityId.Value;
                        mention.ResolutionStatus = ResolutionStatus.Resolved;
                        mention.Confidence = 1.0f; // Manual mapping has high confidence
                    }
                    break;

                default:
                    return Json(new { success = false, message = "Invalid action" });
            }

            await _entityMentionRepository.UpdateAsync(mention);
            return Json(new { success = true, message = "Mention updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mention resolution");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    private EntityType ParseEntityType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return EntityType.Person;
        }

        return type.ToLowerInvariant() switch
        {
            "person" or "character" or "people" => EntityType.Person,
            "place" or "location" => EntityType.Place,
            "org" or "organization" or "faction" => EntityType.Org,
            "object" or "item" or "thing" => EntityType.Object,
            "event" => EntityType.EventLike,
            _ => EntityType.Person
        };
    }
}

/// <summary>
/// DTO for bulk entity import.
/// </summary>
public class ImportEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public List<string>? Aliases { get; set; }
}
