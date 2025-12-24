using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Job tracks background processing tasks like ingestion, dossier rebuilds,
/// reconciliation, and derivation operations.
/// </summary>
public class Job
{
    public Guid JobId { get; set; }
    public Guid UniverseId { get; set; }
    public Guid? DocumentId { get; set; }
    public JobType JobType { get; set; }
    public JobStatus Status { get; set; }
    public string Progress { get; set; } = "{}"; // JSON stored as string
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
    
    // Navigation properties
    public Universe Universe { get; set; } = null!;
}
