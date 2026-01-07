using Microsoft.AspNetCore.Mvc;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Web.Controllers;

/// <summary>
/// Controller for managing documents in the Palimpsest application.
/// </summary>
public class DocumentsController : Controller
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUniverseContextService _universeContext;
    private readonly IIngestionService _ingestionService;
    private readonly ILogger<DocumentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentsController"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="universeContext">The universe context service.</param>
    /// <param name="ingestionService">The ingestion service.</param>
    /// <param name="logger">The logger.</param>
    public DocumentsController(
        IDocumentRepository documentRepository,
        IUniverseContextService universeContext,
        IIngestionService ingestionService,
        ILogger<DocumentsController> logger)
    {
        _documentRepository = documentRepository;
        _universeContext = universeContext;
        _ingestionService = ingestionService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of documents in the active universe.
    /// </summary>
    /// <returns>The view displaying all documents in the active universe.</returns>
    public async Task<IActionResult> Index()
    {
        var activeUniverseId = _universeContext.GetActiveUniverseId();
        if (activeUniverseId == null)
        {
            TempData["Error"] = "Please select an active universe first.";
            return RedirectToAction("Index", "Universes");
        }

        var documents = await _documentRepository.GetByUniverseIdAsync(activeUniverseId.Value);
        ViewBag.ActiveUniverseId = activeUniverseId;
        return View(documents);
    }

    /// <summary>
    /// Displays the form for uploading a new document.
    /// </summary>
    /// <param name="universeId">Optional universe ID to set as active context.</param>
    /// <returns>The view for uploading a document.</returns>
    public IActionResult Upload(Guid? universeId = null)
    {
        // If universeId is provided in query string, set it as active
        if (universeId.HasValue)
        {
            _universeContext.SetActiveUniverseId(universeId.Value);
        }
        
        var activeUniverseId = _universeContext.GetActiveUniverseId();
        if (activeUniverseId == null)
        {
            TempData["Error"] = "Please select an active universe first.";
            return RedirectToAction("Index", "Universes");
        }

        return View();
    }

    /// <summary>
    /// Handles the document upload and optionally starts ingestion.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <param name="title">The document title.</param>
    /// <param name="subtype">The document subtype.</param>
    /// <param name="seriesName">Optional series name.</param>
    /// <param name="bookNumber">Optional book number in series.</param>
    /// <param name="startIngestion">Whether to start ingestion immediately.</param>
    /// <returns>Redirects to the index page on success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        IFormFile file,
        string title,
        DocumentSubtype subtype,
        string? seriesName,
        int? bookNumber,
        bool startIngestion = false)
    {
        var activeUniverseId = _universeContext.GetActiveUniverseId();
        if (activeUniverseId == null)
        {
            TempData["Error"] = "Please select an active universe first.";
            return RedirectToAction("Index", "Universes");
        }

        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Please select a file to upload.");
            return View();
        }

        // Validate file type
        var allowedExtensions = new[] { ".txt", ".md" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError("file", "Only .txt and .md files are supported.");
            return View();
        }

        // Validate file size (max 10MB)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            ModelState.AddModelError("file", "File size must not exceed 10MB.");
            return View();
        }

        try
        {
            // Read file content
            string rawText;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                rawText = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(rawText))
            {
                ModelState.AddModelError("file", "The file appears to be empty.");
                return View();
            }

            // Create document entity
            var document = new Document
            {
                DocumentId = Guid.NewGuid(),
                UniverseId = activeUniverseId.Value,
                Title = title,
                Subtype = subtype,
                SeriesName = seriesName,
                BookNumber = bookNumber,
                Tags = "[]",
                CreatedAt = DateTime.UtcNow
            };

            await _documentRepository.CreateAsync(document);
            _logger.LogInformation("Created document {DocumentId} - {Title}", document.DocumentId, document.Title);

            // Start ingestion if requested
            if (startIngestion)
            {
                try
                {
                    var versionId = await _ingestionService.IngestDocumentAsync(
                        activeUniverseId.Value,
                        document.DocumentId,
                        rawText);
                    
                    _logger.LogInformation("Started ingestion for document {DocumentId}, version {VersionId}", 
                        document.DocumentId, versionId);
                    
                    TempData["Success"] = $"Document '{title}' uploaded and ingestion started successfully.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting ingestion for document {DocumentId}", document.DocumentId);
                    
                    // Roll back the created document to avoid leaving an orphaned document without content
                    try
                    {
                        await _documentRepository.DeleteAsync(document.DocumentId);
                        _logger.LogInformation("Rolled back document {DocumentId} after ingestion failure.", document.DocumentId);
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Failed to roll back document {DocumentId} after ingestion failure.", document.DocumentId);
                    }
                    
                    TempData["Error"] = $"Failed to upload document '{title}' because ingestion could not be started. Please try again.";
                    return View();
                }
            }
            else
            {
                TempData["Success"] = $"Document '{title}' uploaded successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while uploading the document. Please try again later.");
            return View();
        }
    }

    /// <summary>
    /// Displays the details of a specific document.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <returns>The view displaying document details, or NotFound if the document doesn't exist.</returns>
    public async Task<IActionResult> Details(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var activeUniverseId = _universeContext.GetActiveUniverseId();
        if (activeUniverseId == null || document.UniverseId != activeUniverseId.Value)
        {
            return Forbid();
        }

        return View(document);
    }
}
