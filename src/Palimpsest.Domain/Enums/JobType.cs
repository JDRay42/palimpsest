namespace Palimpsest.Domain.Enums;

/// <summary>
/// Types of background jobs.
/// </summary>
public enum JobType
{
    Ingest,
    Rebuild,
    Derive,
    Dossier,
    Reconcile
}
