namespace Palimpsest.Domain.Enums;

/// <summary>
/// Status of background jobs.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job is queued and waiting to be processed.
    /// </summary>
    Queued,
    
    /// <summary>
    /// Job is currently running.
    /// </summary>
    Running,
    
    /// <summary>
    /// Job has completed successfully.
    /// </summary>
    Succeeded,
    
    /// <summary>
    /// Job has failed with an error.
    /// </summary>
    Failed
}
