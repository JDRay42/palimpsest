using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Document represents a book, notes, outline, or appendix within a universe.
/// </summary>
public class Document
{
    public Guid DocumentId { get; set; }
    public Guid UniverseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentSubtype Subtype { get; set; } = DocumentSubtype.Book;
    public string? SeriesName { get; set; }
    public int? BookNumber { get; set; }
    public string Tags { get; set; } = "[]"; // JSON array stored as string
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Universe Universe { get; set; } = null!;
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
