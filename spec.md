# Incremental Canon Engine (V1) — Specification
Version: 0.1  
Target stack: .NET + HTMX/Shoelace, PostgreSQL + pgvector, Docker Compose  
Audience: Maintainers and contributors of the open-source project

## 1. Purpose

Enable authors to ingest very large corpora (hundreds of thousands of words per book; multiple books per universe) and maintain a consistency-aware “canon” that:

- Extracts and stores **entities** and **assertions** with **provenance**.
- Supports **progressive enrichment** (sparse early knowledge becomes richer over time).
- Supports **arbitrary ingestion order** (facts added later can refine or contradict earlier ones).
- Flags **questionable** assertions into a review queue (conflicts, constraint violations, identity ambiguity).
- Produces **entity dossiers** for bounded-context extraction and chat grounding.
- Supports chat and commands within an **active Universe context** (hard boundary).

This engine is “graph-first” (stored in relational tables), with embeddings as retrieval helpers, not the system of record.

---

## 2. Core Concepts

### 2.1 Universe Context
- All operations execute within a selected `universe_id`.
- Hard boundary: no cross-universe retrieval or inference unless explicitly implemented as an advanced feature later.

### 2.2 Assertions are Append-Only
- New knowledge is added; existing assertions are not overwritten.
- “Canon” emerges via conflict detection + author review actions (e.g., mark canon / retcon / unreliable narrator).

### 2.3 Epistemic Categories
Each assertion is categorized as:
- **Observed**: narrator-level or otherwise asserted as factual by the text (as extracted).
- **Believed**: a character believes/says it; may be wrong.
- **Inferred**: derived by the tool (explicitly tracked).

### 2.4 Time Anchors and Scopes
Assertions have:
- **Time Anchor**: where in the narrative this statement is made (document/chapter/header/segment).
- **Time Scope**: when the assertion is true (unknown/exact/range). V1 supports coarse scopes and improves later.

### 2.5 Questionable Queue
An assertion is flagged “questionable” if it:
- conflicts with existing assertions (within overlapping time scope),
- violates a derived constraint,
- has ambiguous identity linkage,
- or is low-confidence under defined thresholds.

---

## 3. V1 Graph Scope

### Included in V1
- **Entities** (people, places, orgs, objects, concepts)
- **Assertions** (S–P–O typed, with provenance, time, epistemic category)
- **Edges** (entity-to-entity relationships denormalized for traversal)
- **Conflicts / Questionable Items**
- **Entity Dossiers** (materialized bounded context for extraction + chat)

### Deferred to V2
- Full event ontology (events as first-class nodes with causality)
- Rich timeline solving (beyond simple ranges/anchors)
- Global, automated canon resolution
- Branching universes / alternate timelines (beyond flags)

---

## 4. Data Model (PostgreSQL)

### 4.1 Required Extensions
- `pgvector` for embeddings
- `pg_trgm` for lexical boosting and name matching

### 4.2 Tables (DDL-level outline)

> Note: this is intentionally explicit enough to implement without further design.

#### universes
- `universe_id uuid pk`
- `name text not null unique`
- `author_name text null` (metadata; may be pseudonym)
- `description text null`
- `created_at timestamptz not null default now()`

#### documents
- `document_id uuid pk`
- `universe_id uuid not null fk -> universes`
- `title text not null`
- `subtype text not null default 'book'`  (book|notes|outline|appendix)
- `series_name text null`
- `book_number int null`
- `tags jsonb not null default '[]'::jsonb`
- `created_at timestamptz not null default now()`

Index: `(universe_id, title)`

#### document_versions
- `version_id uuid pk`
- `document_id uuid not null fk -> documents`
- `ingest_hash text not null` (hash of raw content + normalization version)
- `raw_text text not null`
- `normalized_text text not null`
- `created_at timestamptz not null default now()`

Index: `(document_id, created_at desc)`  
Optional uniqueness: `(document_id, ingest_hash)`

#### segments (chapter/section/paragraphish units)
- `segment_id uuid pk`
- `version_id uuid not null fk -> document_versions`
- `chapter_label text null`          (e.g., "Chapter 2", or date header)
- `section_path text null`           (e.g., "Part I > Chapter 2 > Scene A")
- `ordinal int not null`             (monotonic within version)
- `text text not null`
- `source_locator jsonb not null`    (offsets/line ranges for citations)
- `created_at timestamptz not null default now()`

Index: `(version_id, ordinal)`

#### segment_embeddings
- `segment_id uuid pk fk -> segments`
- `embedding vector(<dims>) not null`
- `model text not null`
- `created_at timestamptz not null default now()`

Vector index appropriate to pgvector setup.

#### entities
- `entity_id uuid pk`
- `universe_id uuid not null fk -> universes`
- `entity_type text not null`        (Person|Place|Org|Object|Concept|EventLike)
- `canonical_name text not null`
- `created_at timestamptz not null default now()`

Index: `(universe_id, entity_type, canonical_name)`

#### entity_aliases
- `alias_id uuid pk`
- `entity_id uuid not null fk -> entities`
- `alias text not null`
- `alias_norm text not null`         (lowercased/diacritics-stripped)
- `confidence real not null default 0.8`
- `created_at timestamptz not null default now()`

Index: `(entity_id, alias_norm)`  
Trigram index: `alias_norm gin_trgm_ops`

#### entity_mentions
- `mention_id uuid pk`
- `universe_id uuid not null fk -> universes`
- `entity_id uuid null fk -> entities`     (null if unresolved mention)
- `segment_id uuid not null fk -> segments`
- `surface_form text not null`
- `span_start int not null`
- `span_end int not null`
- `confidence real not null`
- `resolution_status text not null`        (resolved|candidate|unresolved)
- `created_at timestamptz not null default now()`

Index: `(universe_id, segment_id)` and `(universe_id, entity_id)`

#### assertions
Stores the canonical “truth spine” (append-only).

- `assertion_id uuid pk`
- `universe_id uuid not null fk -> universes`

Subject:
- `subject_entity_id uuid not null fk -> entities`

Predicate:
- `predicate text not null`                (controlled vocabulary recommended)
- `predicate_norm text not null`           (normalized for matching)

Object (typed union):
- `object_kind text not null`              (entity|literal|json)
- `object_entity_id uuid null fk -> entities`
- `object_literal text null`               (string form; if kind=literal)
- `object_type text null`                  (string|int|date|date_md|range|bool|etc.)
- `object_json jsonb null`                 (typed payload; if kind=json)

Epistemic + quality:
- `epistemic text not null`                (Observed|Believed|Inferred)
- `confidence real not null`               (0..1)

Time:
- `time_scope_kind text not null`          (unknown|exact|range)
- `time_exact date null`
- `time_start date null`
- `time_end date null`

Evidence / provenance:
- `evidence_segment_id uuid not null fk -> segments`
- `evidence_excerpt text null`             (optional short excerpt for UI display)
- `created_at timestamptz not null default now()`

Indexes:
- `(universe_id, subject_entity_id, predicate_norm)`
- `(universe_id, predicate_norm)`
- `(universe_id, evidence_segment_id)`

#### edges
Denormalized entity↔entity edges for traversal. Populated from assertions where object_kind=entity.

- `edge_id uuid pk`
- `universe_id uuid not null fk -> universes`
- `from_entity_id uuid not null fk -> entities`
- `to_entity_id uuid not null fk -> entities`
- `relation text not null`                 (often equals predicate_norm)
- `assertion_id uuid not null fk -> assertions`
- `created_at timestamptz not null default now()`

Index: `(universe_id, from_entity_id, relation)`

#### questionable_items
Queue of items requiring author attention (conflicts, identity ambiguity, constraint violations).

- `item_id uuid pk`
- `universe_id uuid not null fk -> universes`
- `item_type text not null`               (conflict|identity|constraint|low_confidence)
- `status text not null`                  (open|dismissed|resolved)
- `severity text not null`                (info|warn|error)
- `subject_entity_id uuid null fk -> entities`
- `assertion_id uuid null fk -> assertions`
- `related_assertion_ids jsonb not null default '[]'::jsonb`
- `details jsonb not null`                (machine + UI metadata)
- `created_at timestamptz not null default now()`
- `resolved_at timestamptz null`
- `resolution jsonb null`                 (author choice / notes)

Index: `(universe_id, status, item_type)`

#### dossiers
Materialized “bounded context packets” per entity (rebuilt when entity is dirty).

- `entity_id uuid pk fk -> entities`
- `universe_id uuid not null fk -> universes`
- `content jsonb not null`                (structured dossier)
- `content_text text not null`            (rendered plain text for LLM prompt)
- `updated_at timestamptz not null default now()`

Index: `(universe_id, updated_at desc)`

#### jobs
- `job_id uuid pk`
- `universe_id uuid not null`
- `document_id uuid null`
- `job_type text not null`                (ingest|rebuild|derive|dossier|reconcile)
- `status text not null`                  (queued|running|succeeded|failed)
- `progress jsonb not null default '{}'::jsonb`
- `created_at timestamptz not null default now()`
- `completed_at timestamptz null`
- `error text null`

Index: `(universe_id, status, created_at desc)`

---

## 5. Controlled Predicate Vocabulary (V1)

V1 supports a limited set of normalized predicates to keep reconciliation tractable. Everything else can be stored as `attribute.*` or `related_to` with typed JSON objects.

### Recommended initial predicates (normalized)
Identity and typing:
- `is_a`                         (Camber is_a Human)
- `alias_of`                     (entity merge aid, rarely direct assertion)
- `named_as`                     (surface naming claim; optional)

Relationships (entity↔entity):
- `parent_of`
- `child_of`
- `grandparent_of`
- `opposed_to`
- `allied_with`
- `member_of`
- `located_in`
- `appears_in`                   (entity↔document concept; can be derived)

Temporal attributes:
- `birthday_day_month`           (object_type = date_md; object_literal like "10-07")
- `age_at`                       (object_json includes age + anchor time if known)

General:
- `attribute`                    (object_json typed payload: { key, value, unit?, notes? })
- `related_to`                   (fallback relation; use sparingly)

---

## 6. Ingestion Pipeline (Graph-First V1)

### 6.1 Ingest Entry Point
`IngestDocument(universe_id, document_id, raw_text, normalization_profile, chunking_profile)`

Produces:
- new `document_version`
- `segments` (chapter/section aware, bounded token size)
- embeddings for each segment
- mentions, entities, assertions (incrementally)
- reconciliation + dossier rebuild

### 6.2 Extraction Stages

#### Stage A: Segmenting
- Parse headings and date headers (lightweight heuristics for txt/md).
- Create `segments` in monotonic order.
- Store `source_locator` including:
  - character offsets in normalized_text
  - line ranges
  - section/chapter labels as captured

#### Stage B: Mention Detection (cheap pass)
- For each segment, detect candidate entity mentions:
  - capitalized sequences
  - known alias matches (trigram)
  - user-defined term lists (future)
- Insert `entity_mentions` as unresolved/candidate.

#### Stage C: Entity Resolution
Resolve each mention to:
- existing entity (high-confidence alias match), or
- new entity (if no good match), or
- unresolved candidate (if ambiguous)

Heuristics:
- exact alias_norm match => resolve
- strong trigram + context overlap => candidate
- otherwise => unresolved

Write:
- entities (when new)
- entity_aliases (seed alias = surface_form)
- entity_mentions updated to resolved/candidate

Create questionable item:
- `identity` if mention has >1 plausible entity above threshold

#### Stage D: Dossier Build (bounded context packet)
Before deep assertion extraction, build a dossier packet for entities in this segment:
- Identify entities present in segment (resolved + top candidates)
- Retrieve existing dossiers (if any)
- Include top-N relevant assertions (by predicate priority + recency + lexical match)
- Include open questionable items for these entities (conflicts/identity)

This packet is what is fed to the LLM to improve extraction accuracy.

#### Stage E: Assertion Extraction (LLM)
For each segment:
Input to LLM:
- segment text
- section/chapter labels and date headers
- entity dossier packet (for entities detected)
- controlled predicate list + output schema

Output:
- list of assertions with:
  - subject name (or entity id if provided)
  - predicate (from vocabulary or `attribute`)
  - object typed
  - epistemic category (Observed/Believed/Inferred; Inferred is discouraged here)
  - confidence
  - time hints if explicit in text (optional)

Persist:
- `assertions`
- `edges` for entity↔entity objects
- optionally new aliases discovered

Note:
- LLM is allowed to emit **Believed** assertions when the text clearly indicates POV belief or dialogue claims.

#### Stage F: Reconcile + Flag Questionable Items
Run after each batch (or after entire doc) to:
- detect predicate conflicts
- detect constraint violations (limited v1 derivations)
- flag low-confidence assertions

Mark impacted entities “dirty” for dossier rebuild.

#### Stage G: Rebuild Dossiers (only dirty entities)
Materialize updated dossiers.

---

## 7. Reconciliation and Conflict Rules (V1)

### 7.1 Conflict Detection
A conflict exists when all are true:
- same `subject_entity_id`
- same `predicate_norm`
- overlapping time scope (or either is unknown time)
- incompatible objects

Incompatibility rules:
- For `is_a`: different types are incompatible unless an allowed hierarchy exists (v1: treat as conflict).
- For `birthday_day_month`: different day/month conflicts (hard).
- For `attribute` with same key: differs materially (string diff; numeric diff beyond tolerance).
- For relation predicates (opposed_to, etc.): may not conflict; usually additive.

If conflict:
- create `questionable_items` with `item_type=conflict`, `severity=warn|error` based on predicate criticality.
- include `related_assertion_ids` + computed explanation in `details`.

### 7.2 Epistemic Precedence (for display, not overwrites)
When summarizing “current view”:
- Observed > Believed > Inferred (unless author marks otherwise)
- but do not discard Believed; it may be narratively important.

### 7.3 Low-Confidence Flagging
If assertion confidence < configured threshold:
- add `questionable_items` type `low_confidence`, severity `info|warn`

### 7.4 Identity Ambiguity Flagging
If a mention has multiple candidate entities above threshold:
- `questionable_items` type `identity`, severity `warn`
- include evidence segments and candidate entity ids

---

## 8. Derivation (V1 Minimal)

V1 includes a small derivation engine to produce useful computed facts and detect constraint violations. These are stored as assertions with `epistemic=Inferred` and `details` that reference their dependency assertions.

### 8.1 Derivation Storage (Optional in V1, recommended)
Add:
- `assertion_dependencies (derived_assertion_id, input_assertion_id)`

### 8.2 Initial Derivation Rules (suggested)
1) **Birthday + age constraint (bounded birth year)**
- Inputs:
  - `birthday_day_month = M-D`
  - `age_at` or a claim like “has not turned 20 by DATE” (modeled as attribute)
- Output:
  - derived `attribute` with key `birth_year_range` or `birth_date_range` when enough info exists.
- If inconsistent, create `questionable_items` type `constraint`, severity `error`.

2) **Appears-in edges**
- If entity is mentioned in a segment of a document:
  - infer `appears_in` relation to document (optional)
  - useful for `/who` and search.

V1 avoids heavy timeline propagation; time scopes remain coarse.

---

## 9. Entity Dossier Construction (V1)

Dossier is a bounded, curated view used for:
- extraction prompting
- chat grounding
- commands like `/who`, `/where`, `/when`

### 9.1 Dossier JSON structure (content)
Example keys:
- `identity`: { canonical_name, type, aliases[] }
- `high_confidence_facts[]`: top facts sorted by predicate priority
- `relationships[]`: edges summary (grandparent_of, opposed_to, etc.)
- `time_notes[]`: any exact/range time-scoped items
- `open_questions[]`: unknown slots (optional heuristic)
- `conflicts[]`: open questionable items relevant to entity
- `top_citations[]`: segment locators for key facts

### 9.2 Dossier Text (content_text)
A plain text rendering suitable for LLM prompts, e.g.:

- Canonical name + aliases
- 10–30 bullet facts with citations (segment ids or human-readable locators)
- 5–20 relationship bullets
- conflict notes (if any)

### 9.3 Dossier Build Algorithm
For a dirty entity:
- Fetch most recent assertions where `confidence >= min_conf`:
  - prioritize predicates in order: `is_a`, kinship, `birthday_day_month`, `opposed_to`, `member_of`, `located_in`, `attribute`
  - include both Observed and Believed, labeled
- Summarize relationships from `edges`
- Attach top evidence locators
- Attach open questionable items
- Write `dossiers` row

---

## 10. “Questionable Facts” Queue UX Contract

### 10.1 Queue Items
Each item must include:
- classification: conflict / identity / constraint / low_confidence
- explanation: short human-readable reason
- evidence: links to segments; show excerpts
- actions:
  - **Resolve** (choose preferred assertion; or merge/split entities; or set status)
  - **Dismiss** (ignore warning, keep item closed)
  - **Defer** (keep open)

### 10.2 Resolution Outcomes (stored as JSON)
Examples:
- Conflict:
  - `{ "resolution": "canon", "canon_assertion_id": "...", "notes": "..." }`
  - `{ "resolution": "retcon", "retconned_assertion_id": "...", "notes": "..." }`
  - `{ "resolution": "unreliable_narrator", "notes": "Believed-only is acceptable" }`
- Identity:
  - `{ "resolution": "merge", "from_entity_id": "...", "into_entity_id": "..." }`
  - `{ "resolution": "keep_separate", "notes": "Different character with same name" }`

Author resolutions must never delete evidence; they annotate canon preference.

---

## 11. Chat Grounding and Commands (Canon Engine Integration)

### 11.1 Universe Context
Chat operates within selected Universe.
Threads are universe-scoped.

### 11.2 Grounding Rules
Chat responses must:
- retrieve relevant dossiers and evidence segments
- cite sources (segment locators)
- prefer Observed assertions when answering “what is true”
- include Believed assertions when user asks about POV beliefs, rumors, or claims

### 11.3 Commands (initial)
- `/who <name>`: resolve entity, show dossier (with citations)
- `/find "<text>"`: lexical search (pg_trgm) returning segments + citations
- `/conflicts`: list open questionable items
- `/scope`: show active scope filters (doc/tag/subtype)
- `/scope add ...` / `/scope clear`
- `/cite <id>`: display cited segments from last response

---

## 12. Configuration Knobs (V1)

- Chunking:
  - max tokens per segment
  - overlap tokens
  - heading sensitivity
- Entity resolution:
  - alias match thresholds
  - candidate ambiguity thresholds
- Extraction:
  - LLM provider selection (Ollama/OpenRouter/etc.)
  - model selection
  - max assertions per segment (to avoid flood)
- Conflict:
  - confidence thresholds
  - predicate criticality map (e.g., birthday is error-level)

---

## 13. Example: Progressive Enrichment (Camber Pattern)

### Phase 1: Ingest “Emerald Eyes” first
System stores sparse assertions:
- `Camber is_a Human` (Observed)
- `Camber grandparent_of POV` (Believed or Observed depending on narration)
- `Camber opposed_to POV in Time Wars` (Believed if POV claims)
- `POV believes his life is in danger if Camber catches him` (Believed)

Dossier is sparse, conflict-free.

### Phase 2: Ingest “The Great Gods” later (earlier timeline)
System adds:
- childhood attributes, relationships, locations, etc.
No overwrite occurs; dossier becomes rich.
If later a statement contradicts “Human,” a conflict is flagged.

Order-independence is achieved because reconciliation compares assertions by subject/predicate/time, not by ingest order.

---

## 14. Acceptance Criteria (V1)

1) Ingest txt/md books up to ~250K words; ≥10 books per universe.
2) Create entities, mentions, assertions, edges with citations.
3) Provide a universe-scoped chat interface that cites evidence.
4) Provide commands `/who`, `/find`, `/conflicts`, `/cite`.
5) Produce a Questionable Facts queue with conflict and identity items.
6) Support universe export as JSON, and universe deletion with strong confirmation.
7) Deterministic provenance: every assertion points to an evidence segment with stable locators.

---

## 15. Implementation Notes

- Graph is stored in Postgres tables; no dedicated graph DB required.
- Keep LLM prompts bounded by using entity dossiers and relevant assertion retrieval.
- Treat extraction as “best effort,” but never allow the LLM to overwrite canon.
- Prefer adding “attribute” assertions rather than inventing new predicates.
- All LLM-derived outputs must be traceable to evidence segments.

End of spec.
