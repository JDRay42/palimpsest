using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Palimpsest.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "universes",
                columns: table => new
                {
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    author_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_universes", x => x.universe_id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    subtype = table.Column<string>(type: "text", nullable: false, defaultValue: "Book"),
                    series_name = table.Column<string>(type: "text", nullable: true),
                    book_number = table.Column<int>(type: "integer", nullable: true),
                    tags = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.document_id);
                    table.ForeignKey(
                        name: "FK_documents_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entities",
                columns: table => new
                {
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    canonical_name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entities", x => x.entity_id);
                    table.ForeignKey(
                        name: "FK_entities_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    progress = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.job_id);
                    table.ForeignKey(
                        name: "FK_jobs_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                columns: table => new
                {
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingest_hash = table.Column<string>(type: "text", nullable: false),
                    raw_text = table.Column<string>(type: "text", nullable: false),
                    normalized_text = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.version_id);
                    table.ForeignKey(
                        name: "FK_document_versions_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "documents",
                        principalColumn: "document_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dossiers",
                columns: table => new
                {
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    content_text = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dossiers", x => x.entity_id);
                    table.ForeignKey(
                        name: "FK_dossiers_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dossiers_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entity_aliases",
                columns: table => new
                {
                    alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias = table.Column<string>(type: "text", nullable: false),
                    alias_norm = table.Column<string>(type: "text", nullable: false),
                    confidence = table.Column<float>(type: "real", nullable: false, defaultValue: 0.8f),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_aliases", x => x.alias_id);
                    table.ForeignKey(
                        name: "FK_entity_aliases_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionable_items",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<string>(type: "text", nullable: false),
                    subject_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assertion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_assertion_ids = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    details = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionable_items", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_questionable_items_entities_subject_entity_id",
                        column: x => x.subject_entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_questionable_items_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "segments",
                columns: table => new
                {
                    segment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chapter_label = table.Column<string>(type: "text", nullable: true),
                    section_path = table.Column<string>(type: "text", nullable: true),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    source_locator = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segments", x => x.segment_id);
                    table.ForeignKey(
                        name: "FK_segments_document_versions_version_id",
                        column: x => x.version_id,
                        principalTable: "document_versions",
                        principalColumn: "version_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assertions",
                columns: table => new
                {
                    assertion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    predicate = table.Column<string>(type: "text", nullable: false),
                    predicate_norm = table.Column<string>(type: "text", nullable: false),
                    object_kind = table.Column<string>(type: "text", nullable: false),
                    object_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    object_literal = table.Column<string>(type: "text", nullable: true),
                    object_type = table.Column<string>(type: "text", nullable: true),
                    object_json = table.Column<string>(type: "jsonb", nullable: true),
                    epistemic = table.Column<string>(type: "text", nullable: false),
                    confidence = table.Column<float>(type: "real", nullable: false),
                    time_scope_kind = table.Column<string>(type: "text", nullable: false),
                    time_exact = table.Column<DateOnly>(type: "date", nullable: true),
                    time_start = table.Column<DateOnly>(type: "date", nullable: true),
                    time_end = table.Column<DateOnly>(type: "date", nullable: true),
                    evidence_segment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evidence_excerpt = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assertions", x => x.assertion_id);
                    table.ForeignKey(
                        name: "FK_assertions_entities_object_entity_id",
                        column: x => x.object_entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_assertions_entities_subject_entity_id",
                        column: x => x.subject_entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assertions_segments_evidence_segment_id",
                        column: x => x.evidence_segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assertions_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entity_mentions",
                columns: table => new
                {
                    mention_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    segment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    surface_form = table.Column<string>(type: "text", nullable: false),
                    span_start = table.Column<int>(type: "integer", nullable: false),
                    span_end = table.Column<int>(type: "integer", nullable: false),
                    confidence = table.Column<float>(type: "real", nullable: false),
                    resolution_status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_mentions", x => x.mention_id);
                    table.ForeignKey(
                        name: "FK_entity_mentions_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_entity_mentions_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entity_mentions_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "segment_embeddings",
                columns: table => new
                {
                    segment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: true),
                    model = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segment_embeddings", x => x.segment_id);
                    table.ForeignKey(
                        name: "FK_segment_embeddings_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "edges",
                columns: table => new
                {
                    edge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    universe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relation = table.Column<string>(type: "text", nullable: false),
                    assertion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edges", x => x.edge_id);
                    table.ForeignKey(
                        name: "FK_edges_assertions_assertion_id",
                        column: x => x.assertion_id,
                        principalTable: "assertions",
                        principalColumn: "assertion_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_edges_entities_from_entity_id",
                        column: x => x.from_entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_edges_entities_to_entity_id",
                        column: x => x.to_entity_id,
                        principalTable: "entities",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_edges_universes_universe_id",
                        column: x => x.universe_id,
                        principalTable: "universes",
                        principalColumn: "universe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assertions_evidence_segment_id",
                table: "assertions",
                column: "evidence_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_assertions_object_entity_id",
                table: "assertions",
                column: "object_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_assertions_subject_entity_id",
                table: "assertions",
                column: "subject_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_assertions_universe_id_evidence_segment_id",
                table: "assertions",
                columns: new[] { "universe_id", "evidence_segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_assertions_universe_id_predicate_norm",
                table: "assertions",
                columns: new[] { "universe_id", "predicate_norm" });

            migrationBuilder.CreateIndex(
                name: "IX_assertions_universe_id_subject_entity_id_predicate_norm",
                table: "assertions",
                columns: new[] { "universe_id", "subject_entity_id", "predicate_norm" });

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id_created_at",
                table: "document_versions",
                columns: new[] { "document_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id_ingest_hash",
                table: "document_versions",
                columns: new[] { "document_id", "ingest_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_universe_id_title",
                table: "documents",
                columns: new[] { "universe_id", "title" });

            migrationBuilder.CreateIndex(
                name: "IX_dossiers_universe_id_updated_at",
                table: "dossiers",
                columns: new[] { "universe_id", "updated_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_edges_assertion_id",
                table: "edges",
                column: "assertion_id");

            migrationBuilder.CreateIndex(
                name: "IX_edges_from_entity_id",
                table: "edges",
                column: "from_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_edges_to_entity_id",
                table: "edges",
                column: "to_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_edges_universe_id_from_entity_id_relation",
                table: "edges",
                columns: new[] { "universe_id", "from_entity_id", "relation" });

            migrationBuilder.CreateIndex(
                name: "IX_entities_universe_id_entity_type_canonical_name",
                table: "entities",
                columns: new[] { "universe_id", "entity_type", "canonical_name" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_aliases_alias_norm",
                table: "entity_aliases",
                column: "alias_norm")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_aliases_entity_id_alias_norm",
                table: "entity_aliases",
                columns: new[] { "entity_id", "alias_norm" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_mentions_entity_id",
                table: "entity_mentions",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_entity_mentions_segment_id",
                table: "entity_mentions",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_entity_mentions_universe_id_entity_id",
                table: "entity_mentions",
                columns: new[] { "universe_id", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_mentions_universe_id_segment_id",
                table: "entity_mentions",
                columns: new[] { "universe_id", "segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_universe_id_status_created_at",
                table: "jobs",
                columns: new[] { "universe_id", "status", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_questionable_items_subject_entity_id",
                table: "questionable_items",
                column: "subject_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_questionable_items_universe_id_status_item_type",
                table: "questionable_items",
                columns: new[] { "universe_id", "status", "item_type" });

            migrationBuilder.CreateIndex(
                name: "IX_segments_version_id_ordinal",
                table: "segments",
                columns: new[] { "version_id", "ordinal" });

            migrationBuilder.CreateIndex(
                name: "IX_universes_name",
                table: "universes",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dossiers");

            migrationBuilder.DropTable(
                name: "edges");

            migrationBuilder.DropTable(
                name: "entity_aliases");

            migrationBuilder.DropTable(
                name: "entity_mentions");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "questionable_items");

            migrationBuilder.DropTable(
                name: "segment_embeddings");

            migrationBuilder.DropTable(
                name: "assertions");

            migrationBuilder.DropTable(
                name: "entities");

            migrationBuilder.DropTable(
                name: "segments");

            migrationBuilder.DropTable(
                name: "document_versions");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "universes");
        }
    }
}
