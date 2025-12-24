using Microsoft.EntityFrameworkCore;
using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;
using Pgvector;

namespace Palimpsest.Infrastructure.Data;

/// <summary>
/// Main database context for Palimpsest.
/// Implements the complete schema as defined in the specification.
/// </summary>
public class PalimpsestDbContext : DbContext
{
    public PalimpsestDbContext(DbContextOptions<PalimpsestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Universe> Universes => Set<Universe>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<Segment> Segments => Set<Segment>();
    public DbSet<SegmentEmbedding> SegmentEmbeddings => Set<SegmentEmbedding>();
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<EntityAlias> EntityAliases => Set<EntityAlias>();
    public DbSet<EntityMention> EntityMentions => Set<EntityMention>();
    public DbSet<Assertion> Assertions => Set<Assertion>();
    public DbSet<Edge> Edges => Set<Edge>();
    public DbSet<QuestionableItem> QuestionableItems => Set<QuestionableItem>();
    public DbSet<Dossier> Dossiers => Set<Dossier>();
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("pg_trgm");

        ConfigureUniverse(modelBuilder);
        ConfigureDocument(modelBuilder);
        ConfigureDocumentVersion(modelBuilder);
        ConfigureSegment(modelBuilder);
        ConfigureSegmentEmbedding(modelBuilder);
        ConfigureEntity(modelBuilder);
        ConfigureEntityAlias(modelBuilder);
        ConfigureEntityMention(modelBuilder);
        ConfigureAssertion(modelBuilder);
        ConfigureEdge(modelBuilder);
        ConfigureQuestionableItem(modelBuilder);
        ConfigureDossier(modelBuilder);
        ConfigureJob(modelBuilder);
    }

    private static void ConfigureUniverse(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Universe>(entity =>
        {
            entity.ToTable("universes");
            entity.HasKey(e => e.UniverseId);
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id");
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired();
            
            entity.Property(e => e.AuthorName)
                .HasColumnName("author_name");
            
            entity.Property(e => e.Description)
                .HasColumnName("description");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private static void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(e => e.DocumentId);
            
            entity.Property(e => e.DocumentId)
                .HasColumnName("document_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired();
            
            entity.Property(e => e.Subtype)
                .HasColumnName("subtype")
                .HasConversion<string>()
                .HasDefaultValue(DocumentSubtype.Book);
            
            entity.Property(e => e.SeriesName)
                .HasColumnName("series_name");
            
            entity.Property(e => e.BookNumber)
                .HasColumnName("book_number");
            
            entity.Property(e => e.Tags)
                .HasColumnName("tags")
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UniverseId, e.Title });

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.Documents)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDocumentVersion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.ToTable("document_versions");
            entity.HasKey(e => e.VersionId);
            
            entity.Property(e => e.VersionId)
                .HasColumnName("version_id");
            
            entity.Property(e => e.DocumentId)
                .HasColumnName("document_id")
                .IsRequired();
            
            entity.Property(e => e.IngestHash)
                .HasColumnName("ingest_hash")
                .IsRequired();
            
            entity.Property(e => e.RawText)
                .HasColumnName("raw_text")
                .IsRequired();
            
            entity.Property(e => e.NormalizedText)
                .HasColumnName("normalized_text")
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.DocumentId, e.CreatedAt })
                .IsDescending(false, true);

            entity.HasIndex(e => new { e.DocumentId, e.IngestHash }).IsUnique();

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Versions)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSegment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Segment>(entity =>
        {
            entity.ToTable("segments");
            entity.HasKey(e => e.SegmentId);
            
            entity.Property(e => e.SegmentId)
                .HasColumnName("segment_id");
            
            entity.Property(e => e.VersionId)
                .HasColumnName("version_id")
                .IsRequired();
            
            entity.Property(e => e.ChapterLabel)
                .HasColumnName("chapter_label");
            
            entity.Property(e => e.SectionPath)
                .HasColumnName("section_path");
            
            entity.Property(e => e.Ordinal)
                .HasColumnName("ordinal")
                .IsRequired();
            
            entity.Property(e => e.Text)
                .HasColumnName("text")
                .IsRequired();
            
            entity.Property(e => e.SourceLocator)
                .HasColumnName("source_locator")
                .HasColumnType("jsonb")
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.VersionId, e.Ordinal });

            entity.HasOne(e => e.Version)
                .WithMany(v => v.Segments)
                .HasForeignKey(e => e.VersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSegmentEmbedding(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SegmentEmbedding>(entity =>
        {
            entity.ToTable("segment_embeddings");
            entity.HasKey(e => e.SegmentId);
            
            entity.Property(e => e.SegmentId)
                .HasColumnName("segment_id");
            
            // Configure vector property with conversion
            entity.Property(e => e.Embedding)
                .HasColumnName("embedding")
                .HasColumnType("vector(384)")
                .HasConversion(
                    v => v == null ? null : new Vector(v),
                    v => v == null ? null : v.ToArray());
            
            entity.Property(e => e.Model)
                .HasColumnName("model")
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasOne(e => e.Segment)
                .WithOne(s => s.Embedding)
                .HasForeignKey<SegmentEmbedding>(e => e.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>(entity =>
        {
            entity.ToTable("entities");
            entity.HasKey(e => e.EntityId);
            
            entity.Property(e => e.EntityId)
                .HasColumnName("entity_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.EntityType)
                .HasColumnName("entity_type")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.CanonicalName)
                .HasColumnName("canonical_name")
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UniverseId, e.EntityType, e.CanonicalName });

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.Entities)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEntityAlias(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityAlias>(entity =>
        {
            entity.ToTable("entity_aliases");
            entity.HasKey(e => e.AliasId);
            
            entity.Property(e => e.AliasId)
                .HasColumnName("alias_id");
            
            entity.Property(e => e.EntityId)
                .HasColumnName("entity_id")
                .IsRequired();
            
            entity.Property(e => e.Alias)
                .HasColumnName("alias")
                .IsRequired();
            
            entity.Property(e => e.AliasNorm)
                .HasColumnName("alias_norm")
                .IsRequired();
            
            entity.Property(e => e.Confidence)
                .HasColumnName("confidence")
                .HasDefaultValue(0.8f);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.EntityId, e.AliasNorm });
            
            // Trigram index for fuzzy matching
            entity.HasIndex(e => e.AliasNorm)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasOne(e => e.Entity)
                .WithMany(ent => ent.Aliases)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEntityMention(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityMention>(entity =>
        {
            entity.ToTable("entity_mentions");
            entity.HasKey(e => e.MentionId);
            
            entity.Property(e => e.MentionId)
                .HasColumnName("mention_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.EntityId)
                .HasColumnName("entity_id");
            
            entity.Property(e => e.SegmentId)
                .HasColumnName("segment_id")
                .IsRequired();
            
            entity.Property(e => e.SurfaceForm)
                .HasColumnName("surface_form")
                .IsRequired();
            
            entity.Property(e => e.SpanStart)
                .HasColumnName("span_start")
                .IsRequired();
            
            entity.Property(e => e.SpanEnd)
                .HasColumnName("span_end")
                .IsRequired();
            
            entity.Property(e => e.Confidence)
                .HasColumnName("confidence")
                .IsRequired();
            
            entity.Property(e => e.ResolutionStatus)
                .HasColumnName("resolution_status")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UniverseId, e.SegmentId });
            entity.HasIndex(e => new { e.UniverseId, e.EntityId });

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.EntityMentions)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Entity)
                .WithMany(ent => ent.Mentions)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Segment)
                .WithMany(s => s.Mentions)
                .HasForeignKey(e => e.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAssertion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assertion>(entity =>
        {
            entity.ToTable("assertions");
            entity.HasKey(e => e.AssertionId);
            
            entity.Property(e => e.AssertionId)
                .HasColumnName("assertion_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.SubjectEntityId)
                .HasColumnName("subject_entity_id")
                .IsRequired();
            
            entity.Property(e => e.Predicate)
                .HasColumnName("predicate")
                .IsRequired();
            
            entity.Property(e => e.PredicateNorm)
                .HasColumnName("predicate_norm")
                .IsRequired();
            
            entity.Property(e => e.ObjectKind)
                .HasColumnName("object_kind")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.ObjectEntityId)
                .HasColumnName("object_entity_id");
            
            entity.Property(e => e.ObjectLiteral)
                .HasColumnName("object_literal");
            
            entity.Property(e => e.ObjectType)
                .HasColumnName("object_type");
            
            entity.Property(e => e.ObjectJson)
                .HasColumnName("object_json")
                .HasColumnType("jsonb");
            
            entity.Property(e => e.Epistemic)
                .HasColumnName("epistemic")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.Confidence)
                .HasColumnName("confidence")
                .IsRequired();
            
            entity.Property(e => e.TimeScopeKind)
                .HasColumnName("time_scope_kind")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.TimeExact)
                .HasColumnName("time_exact");
            
            entity.Property(e => e.TimeStart)
                .HasColumnName("time_start");
            
            entity.Property(e => e.TimeEnd)
                .HasColumnName("time_end");
            
            entity.Property(e => e.EvidenceSegmentId)
                .HasColumnName("evidence_segment_id")
                .IsRequired();
            
            entity.Property(e => e.EvidenceExcerpt)
                .HasColumnName("evidence_excerpt");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UniverseId, e.SubjectEntityId, e.PredicateNorm });
            entity.HasIndex(e => new { e.UniverseId, e.PredicateNorm });
            entity.HasIndex(e => new { e.UniverseId, e.EvidenceSegmentId });

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.Assertions)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SubjectEntity)
                .WithMany(ent => ent.SubjectAssertions)
                .HasForeignKey(e => e.SubjectEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ObjectEntity)
                .WithMany(ent => ent.ObjectAssertions)
                .HasForeignKey(e => e.ObjectEntityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.EvidenceSegment)
                .WithMany(s => s.Assertions)
                .HasForeignKey(e => e.EvidenceSegmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEdge(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Edge>(entity =>
        {
            entity.ToTable("edges");
            entity.HasKey(e => e.EdgeId);
            
            entity.Property(e => e.EdgeId)
                .HasColumnName("edge_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.FromEntityId)
                .HasColumnName("from_entity_id")
                .IsRequired();
            
            entity.Property(e => e.ToEntityId)
                .HasColumnName("to_entity_id")
                .IsRequired();
            
            entity.Property(e => e.Relation)
                .HasColumnName("relation")
                .IsRequired();
            
            entity.Property(e => e.AssertionId)
                .HasColumnName("assertion_id")
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UniverseId, e.FromEntityId, e.Relation });

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.Edges)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FromEntity)
                .WithMany(ent => ent.EdgesFrom)
                .HasForeignKey(e => e.FromEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ToEntity)
                .WithMany(ent => ent.EdgesTo)
                .HasForeignKey(e => e.ToEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Assertion)
                .WithMany(a => a.Edges)
                .HasForeignKey(e => e.AssertionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureQuestionableItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuestionableItem>(entity =>
        {
            entity.ToTable("questionable_items");
            entity.HasKey(e => e.ItemId);
            
            entity.Property(e => e.ItemId)
                .HasColumnName("item_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.ItemType)
                .HasColumnName("item_type")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.Severity)
                .HasColumnName("severity")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.SubjectEntityId)
                .HasColumnName("subject_entity_id");
            
            entity.Property(e => e.AssertionId)
                .HasColumnName("assertion_id");
            
            entity.Property(e => e.RelatedAssertionIds)
                .HasColumnName("related_assertion_ids")
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");
            
            entity.Property(e => e.Details)
                .HasColumnName("details")
                .HasColumnType("jsonb")
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");
            
            entity.Property(e => e.ResolvedAt)
                .HasColumnName("resolved_at");
            
            entity.Property(e => e.Resolution)
                .HasColumnName("resolution")
                .HasColumnType("jsonb");

            entity.HasIndex(e => new { e.UniverseId, e.Status, e.ItemType });

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.QuestionableItems)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SubjectEntity)
                .WithMany()
                .HasForeignKey(e => e.SubjectEntityId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureDossier(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dossier>(entity =>
        {
            entity.ToTable("dossiers");
            entity.HasKey(e => e.EntityId);
            
            entity.Property(e => e.EntityId)
                .HasColumnName("entity_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.Content)
                .HasColumnName("content")
                .HasColumnType("jsonb")
                .IsRequired();
            
            entity.Property(e => e.ContentText)
                .HasColumnName("content_text")
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UniverseId, e.UpdatedAt })
                .IsDescending(false, true);

            entity.HasOne(e => e.Entity)
                .WithOne(ent => ent.Dossier)
                .HasForeignKey<Dossier>(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.Dossiers)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");
            entity.HasKey(e => e.JobId);
            
            entity.Property(e => e.JobId)
                .HasColumnName("job_id");
            
            entity.Property(e => e.UniverseId)
                .HasColumnName("universe_id")
                .IsRequired();
            
            entity.Property(e => e.DocumentId)
                .HasColumnName("document_id");
            
            entity.Property(e => e.JobType)
                .HasColumnName("job_type")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();
            
            entity.Property(e => e.Progress)
                .HasColumnName("progress")
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");
            
            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at");
            
            entity.Property(e => e.Error)
                .HasColumnName("error");

            entity.HasIndex(e => new { e.UniverseId, e.Status, e.CreatedAt })
                .IsDescending(false, false, true);

            entity.HasOne(e => e.Universe)
                .WithMany(u => u.Jobs)
                .HasForeignKey(e => e.UniverseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
