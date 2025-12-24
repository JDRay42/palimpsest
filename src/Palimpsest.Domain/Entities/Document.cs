using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Document represents a book, notes, outline, or appendix within a universe.
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    public Guid DocumentId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this document belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the title of the document.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subtype classification of the document.
    /// </summary>
    public DocumentSubtype Subtype { get; set; } = DocumentSubtype.Book;
    
    /// <summary>
    /// Gets or sets the optional series name if this document is part of a series.
    /// </summary>
    public string? SeriesName { get; set; }
    
    /// <summary>
    /// Gets or sets the optional book number if this document is part of a series.
    /// </summary>
    public int? BookNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the tags as a JSON array stored as string.
    /// </summary>
    public string Tags { get; set; } = "[]";
    
    /// <summary>
    /// Gets or sets the timestamp when this document was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the universe this document belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the collection of versions of this document.
    /// </summary>
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
