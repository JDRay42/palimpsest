using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Job tracks background processing tasks like ingestion, dossier rebuilds,
/// reconciliation, and derivation operations.
/// </summary>
public class Job
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public Guid JobId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this job belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the optional identifier of the document this job is processing.
    /// </summary>
    public Guid? DocumentId { get; set; }
    
    /// <summary>
    /// Gets or sets the type of background job being performed.
    /// </summary>
    public JobType JobType { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of the job.
    /// </summary>
    public JobStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the progress information as JSON stored as string.
    /// </summary>
    public string Progress { get; set; } = "{}";
    
    /// <summary>
    /// Gets or sets the timestamp when this job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this job completed, if applicable.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if the job failed.
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// Gets or sets the universe this job belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
}
