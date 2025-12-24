namespace Palimpsest.Domain.Enums;

/// <summary>
/// Status of background jobs.
/// </summary>
public enum JobStatus
{
    Queued,
    Running,
    Succeeded,
    Failed
}
