namespace Palimpsest.Domain.Enums;

/// <summary>
/// Types of background jobs.
/// </summary>
public enum JobType
{
    /// <summary>
    /// Document ingestion job.
    /// </summary>
    Ingest,
    
    /// <summary>
    /// Rebuild job for reconstructing data structures.
    /// </summary>
    Rebuild,
    
    /// <summary>
    /// Derivation job for inferring new assertions.
    /// </summary>
    Derive,
    
    /// <summary>
    /// Dossier generation or update job.
    /// </summary>
    Dossier,
    
    /// <summary>
    /// Reconciliation job for resolving conflicts.
    /// </summary>
    Reconcile
}
