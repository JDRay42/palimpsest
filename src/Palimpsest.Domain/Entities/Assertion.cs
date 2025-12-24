using Palimpsest.Domain.Enums;

namespace Palimpsest.Domain.Entities;

/// <summary>
/// Assertion is the fundamental unit of canon: a subject-predicate-object triple
/// with epistemic category, confidence, time scope, and mandatory provenance.
/// Assertions are append-only; conflicts are flagged, not overwritten.
/// </summary>
public class Assertion
{
    /// <summary>
    /// Gets or sets the unique identifier for the assertion.
    /// </summary>
    public Guid AssertionId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the universe this assertion belongs to.
    /// </summary>
    public Guid UniverseId { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the subject entity in this assertion.
    /// </summary>
    public Guid SubjectEntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the predicate (relationship or property) in this assertion.
    /// </summary>
    public string Predicate { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the normalized form of the predicate for matching.
    /// </summary>
    public string PredicateNorm { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the kind of object in this assertion (Entity, Literal, or Json).
    /// </summary>
    public ObjectKind ObjectKind { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the object entity, if ObjectKind is Entity.
    /// </summary>
    public Guid? ObjectEntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the literal value of the object, if ObjectKind is Literal.
    /// </summary>
    public string? ObjectLiteral { get; set; }
    
    /// <summary>
    /// Gets or sets the type of the object for type discrimination.
    /// </summary>
    public string? ObjectType { get; set; }
    
    /// <summary>
    /// Gets or sets the JSON representation of the object, if ObjectKind is Json.
    /// </summary>
    public string? ObjectJson { get; set; }
    
    /// <summary>
    /// Gets or sets the epistemic category indicating the certainty level of this assertion.
    /// </summary>
    public EpistemicCategory Epistemic { get; set; }
    
    /// <summary>
    /// Gets or sets the confidence score for this assertion (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; set; }
    
    /// <summary>
    /// Gets or sets the kind of time scope for this assertion (Unknown, Exact, or Range).
    /// </summary>
    public TimeScopeKind TimeScopeKind { get; set; }
    
    /// <summary>
    /// Gets or sets the exact date if TimeScopeKind is Exact.
    /// </summary>
    public DateOnly? TimeExact { get; set; }
    
    /// <summary>
    /// Gets or sets the start date if TimeScopeKind is Range.
    /// </summary>
    public DateOnly? TimeStart { get; set; }
    
    /// <summary>
    /// Gets or sets the end date if TimeScopeKind is Range.
    /// </summary>
    public DateOnly? TimeEnd { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the segment providing evidence for this assertion.
    /// </summary>
    public Guid EvidenceSegmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the optional excerpt from the evidence segment supporting this assertion.
    /// </summary>
    public string? EvidenceExcerpt { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this assertion was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the universe this assertion belongs to.
    /// </summary>
    public Universe Universe { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the subject entity of this assertion.
    /// </summary>
    public Entity SubjectEntity { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the object entity if ObjectKind is Entity.
    /// </summary>
    public Entity? ObjectEntity { get; set; }
    
    /// <summary>
    /// Gets or sets the segment providing evidence for this assertion.
    /// </summary>
    public Segment EvidenceSegment { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the collection of edges derived from this assertion.
    /// </summary>
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
}
