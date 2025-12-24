using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Assertion is the fundamental unit of canon: a subject-predicate-object triple
/// with epistemic category, confidence, time scope, and mandatory provenance.
/// Assertions are append-only; conflicts are flagged, not overwritten.
/// </summary>
public class Assertion
{
    public Guid AssertionId { get; set; }
    public Guid UniverseId { get; set; }
    
    // Subject
    public Guid SubjectEntityId { get; set; }
    
    // Predicate
    public string Predicate { get; set; } = string.Empty;
    public string PredicateNorm { get; set; } = string.Empty;
    
    // Object (typed union)
    public ObjectKind ObjectKind { get; set; }
    public Guid? ObjectEntityId { get; set; }
    public string? ObjectLiteral { get; set; }
    public string? ObjectType { get; set; }
    public string? ObjectJson { get; set; }
    
    // Epistemic + quality
    public EpistemicCategory Epistemic { get; set; }
    public float Confidence { get; set; }
    
    // Time scope
    public TimeScopeKind TimeScopeKind { get; set; }
    public DateOnly? TimeExact { get; set; }
    public DateOnly? TimeStart { get; set; }
    public DateOnly? TimeEnd { get; set; }
    
    // Evidence / provenance
    public Guid EvidenceSegmentId { get; set; }
    public string? EvidenceExcerpt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Universe Universe { get; set; } = null!;
    public Entity SubjectEntity { get; set; } = null!;
    public Entity? ObjectEntity { get; set; }
    public Segment EvidenceSegment { get; set; } = null!;
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
}
