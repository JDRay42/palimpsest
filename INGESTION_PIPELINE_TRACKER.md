# Ingestion Pipeline Implementation Tracker

**Project**: Palimpsest - Incremental Canon Engine  
**Epic**: Complete Document Ingestion & Entity Extraction Pipeline  
**Status**: ✅ Phase 2 Complete → Phase 3-5 Pending  
**Last Updated**: January 5, 2026

---

## Overview

This tracker documents the implementation of the full document ingestion pipeline as specified in the project spec. The pipeline transforms raw text documents into a structured knowledge graph with entities, assertions, relationships, and conflict detection.

### Current Status: ✅ Phase 2 COMPLETE
- ✅ Document creation and upload working
- ✅ Text normalization implemented
- ✅ Basic segmentation (paragraph-based)
- ✅ Job tracking with progress reporting
- ✅ Entity mention detection (capitalization-based with stopword filtering)
- ✅ Entity resolution with fuzzy matching (pg_trgm)
- ✅ QuestionableItem creation for ambiguous entities
- ✅ Entity CRUD operations with bulk import (JSON/CSV)
- ✅ Mention validation UI with approve/reject/remap
- ✅ QuestionableItem resolution workflow
- ✅ **49 passing unit tests** covering all NER functionality

### Remaining Work: Phases 3-5
- Dossier generation
- LLM-based assertion extraction
- Conflict detection & reconciliation

---

## Phase 2: Entity Mention Detection & Resolution

**Goal**: Identify potential entities in text and resolve them to canonical entity records.

### Task 2.1: Mention Detection Service ✅ COMPLETE
**Priority**: HIGH  
**Complexity**: Medium  
**Dependencies**: None  
**Actual Effort**: 6 hours

**Requirements**:
- Create `IEntityMentionService` interface in Application layer
- Implement `EntityMentionService` in Infrastructure layer
- Detect capitalized sequences as potential entity mentions
- Store character offsets (span_start, span_end) in segment text
- Create `EntityMention` records with `resolution_status = 'unresolved'`
- Support basic filtering (ignore common words like "The", "A", etc.)

**Acceptance Criteria**:
- [x] Service can scan a segment and return list of mention candidates
- [x] Mentions include surface form, character spans, and confidence
- [x] Mentions are persisted to `entity_mentions` table
- [x] Unit tests for mention detection logic (14 tests)
- [x] Integration test scaffolding created

**Files Created/Modified**:
- ✅ `src/Palimpsest.Application/Interfaces/Services/IEntityMentionService.cs`
- ✅ `src/Palimpsest.Infrastructure/Services/EntityMentionService.cs`
- ✅ `src/Palimpsest.Infrastructure/Repositories/EntityMentionRepository.cs`
- ✅ `src/Palimpsest.Tests.Unit/Services/EntityMentionServiceTests.cs`

---

### Task 2.2: Entity Alias Matching ✅ COMPLETE
**Priority**: HIGH  
**Complexity**: Medium  
**Dependencies**: Task 2.1  
**Actual Effort**: 4 hours

**Requirements**:
- Implement fuzzy matching using PostgreSQL `pg_trgm` extension
- Add trigram index to `entity_aliases.alias_norm` if not present
- Create similarity search for alias matching
- Support exact match (normalized) as highest confidence
- Support fuzzy match with configurable threshold (default 0.75)

**Acceptance Criteria**:
- [x] Can find exact alias matches with confidence = 1.0
- [x] Can find similar aliases with graded confidence
- [x] Uses database trigram indexes for performance
- [x] Returns top N matches with confidence scores
- [x] Unit tests for matching logic (7 tests)

**Files Modified**:
- ✅ `src/Palimpsest.Infrastructure/Repositories/EntityAliasRepository.cs`
- ✅ Added `FindMatchingEntitiesAsync(string surfaceForm, float threshold, int maxResults)`
- ✅ `src/Palimpsest.Tests.Unit/Repositories/EntityAliasRepositoryTests.cs`

---

### Task 2.3: Entity Resolution Logic ✅ COMPLETE
**Priority**: HIGH  
**Complexity**: Medium-High  
**Dependencies**: Task 2.1, Task 2.2  
**Actual Effort**: 8 hours

**Requirements**:
- Implement entity resolution rules:
  - Exact alias match (normalized) → resolve to entity
  - Single fuzzy match above threshold → resolve as candidate
  - Multiple matches → create questionable item (identity ambiguity)
  - No matches → create new entity
- Update `EntityMention.resolution_status` appropriately
- Link `EntityMention.entity_id` when resolved
- Create new entities with canonical name = first surface form
- Create initial entity alias for new entities

**Acceptance Criteria**:
- [x] Exact matches resolve automatically
- [x] Ambiguous matches create questionable items
- [x] New entities created for unmatched mentions
- [x] Resolution status correctly set (resolved/candidate/unresolved)
- [x] Integration test scaffolding created (28 tests total)

**Files Created**:
- ✅ `src/Palimpsest.Infrastructure/Services/EntityResolutionService.cs`
- ✅ `src/Palimpsest.Application/Interfaces/Services/IEntityResolutionService.cs`
- ✅ `src/Palimpsest.Tests.Unit/Services/EntityResolutionServiceTests.cs`

---

### Task 2.4: Integrate Mention Detection into Ingestion Pipeline ✅ COMPLETE
**Priority**: HIGH  
**Complexity**: Low  
**Dependencies**: Task 2.1, Task 2.2, Task 2.3  
**Actual Effort**: 3 hours

**Requirements**:
- Update `IngestionService.IngestDocumentAsync()` to call mention detection
- Process each segment for mentions after segmentation
- Run entity resolution on detected mentions
- Update job progress JSON to track mention detection stage
- Handle errors gracefully (log and continue)

**Acceptance Criteria**:
- [x] Ingestion pipeline calls mention detection automatically
- [x] Mentions persisted to database
- [x] Entity resolution runs for all mentions
- [x] Job progress shows mention detection stage
- [x] Error handling prevents pipeline failure

**Files Modified**:
- ✅ `src/Palimpsest.Infrastructure/Services/IngestionService.cs`

---

### Task 2.5: Entity Management UI ✅ COMPLETE
**Priority**: HIGH  
**Complexity**: Medium-High  
**Dependencies**: Tasks 2.1-2.4  
**Actual Effort**: 12 hours

**Requirements**:
- Create entity CRUD operations (Create, Edit, Delete)
- Implement bulk import from JSON and CSV formats
- Build mention validation UI with approve/reject/remap
- Implement QuestionableItem resolution workflow
- Add entity search and filtering

**Acceptance Criteria**:
- [x] Manual entity creation with aliases before document ingestion
- [x] Bulk import supports JSON array and CSV formats
- [x] Edit entity with alias diff logic (add/remove)
- [x] Safe deletion with mention unlinking
- [x] Mention validation UI in entity details with AJAX actions
- [x] QuestionableItem index with status filtering
- [x] QuestionableItem details with resolution options
- [x] Entity search for remapping mentions

**Files Created**:
- ✅ `src/Palimpsest.Web/Controllers/EntitiesController.cs` (enhanced with CRUD)
- ✅ `src/Palimpsest.Web/Controllers/QuestionableController.cs` (implemented)
- ✅ `src/Palimpsest.Web/Views/Entities/Create.cshtml`
- ✅ `src/Palimpsest.Web/Views/Entities/Edit.cshtml`
- ✅ `src/Palimpsest.Web/Views/Entities/Delete.cshtml`
- ✅ `src/Palimpsest.Web/Views/Entities/Import.cshtml`
- ✅ `src/Palimpsest.Web/Views/Entities/Details.cshtml` (enhanced with mentions tab)
- ✅ `src/Palimpsest.Web/Views/Entities/Index.cshtml` (enhanced with create/import buttons)
- ✅ `src/Palimpsest.Web/Views/Questionable/Index.cshtml` (redesigned)
- ✅ `src/Palimpsest.Web/Views/Questionable/Details.cshtml`
- ✅ `src/Palimpsest.Application/DTOs/ImportEntityDto.cs`
- ✅ Enhanced EntityAliasRepository with UpdateAsync/DeleteAsync

**User Workflows Enabled**:
1. **Pre-Ingestion**: Import character list → entities created with aliases → ready for NER matching
2. **Post-Ingestion**: Review mentions → approve/reject/remap → validate entity links
3. **Ambiguity Resolution**: Review QuestionableItems → link/create/dismiss → resolve conflicts

---

## Phase 3: Dossier System

**Goal**: Create bounded context packets for entities to support accurate assertion extraction.

### Task 3.1: Dossier Data Model & Repository ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Low-Medium  
**Dependencies**: Phase 2 complete  
**Estimated Effort**: 2-3 hours

**Requirements**:
- Verify `Dossier` entity model matches spec
- Implement `IDossierRepository` interface
- Create `DossierRepository` with CRUD operations
- Support querying by entity_id and universe_id
- Include methods for marking entities as "dirty"

**Acceptance Criteria**:
- [ ] Repository supports creating/updating dossiers
- [ ] Can retrieve dossier by entity_id
- [ ] Can query dirty dossiers for rebuild
- [ ] Unit tests for repository operations

**Files to Create/Modify**:
- `src/Palimpsest.Application/Interfaces/Repositories/IDossierRepository.cs` (verify)
- `src/Palimpsest.Infrastructure/Repositories/DossierRepository.cs` (implement)

---

### Task 3.2: Dossier Builder Service ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Medium-High  
**Dependencies**: Task 3.1  
**Estimated Effort**: 6-8 hours

**Requirements**:
- Create `IDossierService` interface
- Implement dossier building logic:
  - Collect entity metadata (canonical name, type, aliases)
  - Retrieve top-N relevant assertions (by recency and confidence)
  - Include entity relationships (edges)
  - Include open questionable items for entity
  - Include mention context from segments
- Generate structured JSON content
- Generate plain text summary (content_text) for LLM consumption
- Support partial rebuilds (only dirty entities)

**Acceptance Criteria**:
- [ ] Can build complete dossier for an entity
- [ ] JSON structure is well-organized and complete
- [ ] Plain text format is LLM-friendly
- [ ] Handles entities with no assertions gracefully
- [ ] Performance is acceptable (< 500ms per entity)
- [ ] Unit and integration tests

**Files to Create/Modify**:
- `src/Palimpsest.Application/Interfaces/Services/IDossierService.cs` (new)
- `src/Palimpsest.Infrastructure/Services/DossierService.cs` (new)

---

### Task 3.3: Dossier Rebuild Job ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Low-Medium  
**Dependencies**: Task 3.2  
**Estimated Effort**: 3-4 hours

**Requirements**:
- Create background job type for dossier rebuilds
- Support batch rebuilding (multiple dirty entities)
- Track progress in job.progress JSON
- Mark entities as clean after successful rebuild
- Handle failures and retries

**Acceptance Criteria**:
- [ ] Can trigger dossier rebuild job
- [ ] Job processes multiple entities efficiently
- [ ] Progress tracking works correctly
- [ ] Entities marked clean after rebuild
- [ ] Failed entities logged but don't block batch

**Files to Modify**:
- `src/Palimpsest.Infrastructure/Services/JobProcessingService.cs` (new or enhance existing)
- `src/Palimpsest.Domain/Enums/JobType.cs` (add Dossier if needed)

---

## Phase 4: Assertion Extraction (LLM Integration)

**Goal**: Use LLM to extract structured facts from text segments.

### Task 4.1: LLM Service Interface & Configuration ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: Medium  
**Dependencies**: None (can be done in parallel)  
**Estimated Effort**: 3-4 hours

**Requirements**:
- Create `ILLMService` interface in Application layer
- Support multiple LLM providers (OpenAI, Anthropic, Azure OpenAI)
- Configuration for API keys, endpoints, models
- Add settings to appsettings.json
- Support structured output (JSON schema enforcement)
- Include retry logic and rate limiting

**Acceptance Criteria**:
- [ ] Interface supports sending prompts and receiving structured responses
- [ ] Configuration supports multiple providers
- [ ] Can inject appropriate provider based on config
- [ ] Basic error handling and retries
- [ ] Unit tests with mock LLM responses

**Files to Create**:
- `src/Palimpsest.Application/Interfaces/Services/ILLMService.cs` (new)
- `src/Palimpsest.Infrastructure/Services/OpenAIService.cs` (new)
- `src/Palimpsest.Infrastructure/Services/AnthropicService.cs` (new - optional)
- `src/Palimpsest.Web/appsettings.json` (update)

---

### Task 4.2: Assertion Extraction Prompts ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: Medium-High  
**Dependencies**: Task 4.1, Phase 3 complete  
**Estimated Effort**: 4-6 hours

**Requirements**:
- Design prompt template for assertion extraction
- Include controlled predicate vocabulary in prompt
- Include entity dossiers for context
- Specify JSON output schema:
  ```json
  {
    "assertions": [
      {
        "subject": "entity name or id",
        "predicate": "predicate from vocabulary",
        "object": {"kind": "entity|literal|json", "value": "..."},
        "epistemic": "Observed|Believed|Inferred",
        "confidence": 0.0-1.0,
        "time_scope": "unknown|exact|range",
        "evidence_excerpt": "quote from text"
      }
    ]
  }
  ```
- Handle epistemic categories correctly (Believed for dialogue/POV)
- Extract time information when present
- Test prompt with various text types

**Acceptance Criteria**:
- [ ] Prompt template is clear and comprehensive
- [ ] LLM reliably returns valid JSON
- [ ] Extracts appropriate predicates from vocabulary
- [ ] Correctly identifies epistemic categories
- [ ] Handles various text formats (narrative, dialogue, exposition)
- [ ] Manual testing with 10+ diverse segments

**Files to Create**:
- `src/Palimpsest.Infrastructure/Services/Prompts/AssertionExtractionPrompt.cs` (new)
- Store templates in embedded resources or config

---

### Task 4.3: Assertion Extraction Service ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: High  
**Dependencies**: Task 4.1, Task 4.2, Phase 3 complete  
**Estimated Effort**: 8-10 hours

**Requirements**:
- Create `IAssertionExtractionService` interface
- Implement extraction logic:
  - For each segment, identify relevant entities
  - Build dossier packet for those entities
  - Construct LLM prompt with segment + dossiers
  - Call LLM service
  - Parse JSON response
  - Validate extractions against schema
  - Resolve entity references (names → entity_ids)
  - Create `Assertion` records
  - Create `Edge` records for entity-entity assertions
  - Create new entity aliases if discovered
- Handle LLM errors and malformed responses
- Support batch processing for efficiency

**Acceptance Criteria**:
- [ ] Can extract assertions from a segment
- [ ] Dossier context included in LLM call
- [ ] Assertions persisted with all required fields
- [ ] Edges created for entity relationships
- [ ] New aliases registered when appropriate
- [ ] Handles errors gracefully
- [ ] Integration test with real LLM (or mock)
- [ ] Performance: < 5 seconds per segment (LLM dependent)

**Files to Create**:
- `src/Palimpsest.Application/Interfaces/Services/IAssertionExtractionService.cs` (new)
- `src/Palimpsest.Infrastructure/Services/AssertionExtractionService.cs` (new)

---

### Task 4.4: Integrate Assertion Extraction into Pipeline ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: Medium  
**Dependencies**: Task 4.3  
**Estimated Effort**: 3-4 hours

**Requirements**:
- Update `IngestionService.IngestDocumentAsync()` to call assertion extraction
- Process segments sequentially or in small batches
- Update job progress JSON to track extraction stage
- Track tokens used and costs
- Handle rate limits and quota errors
- Support resume from last processed segment on retry

**Acceptance Criteria**:
- [ ] Pipeline calls assertion extraction after mention detection
- [ ] Job progress shows extraction status
- [ ] Can resume failed jobs
- [ ] Rate limiting prevents API errors
- [ ] Cost tracking in job metadata

**Files to Modify**:
- `src/Palimpsest.Infrastructure/Services/IngestionService.cs`

---

## Phase 5: Conflict Detection & Reconciliation

**Goal**: Identify contradictions and create review queue for author.

### Task 5.1: Conflict Detection Rules Engine ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: High  
**Dependencies**: Phase 4 complete  
**Estimated Effort**: 8-10 hours

**Requirements**:
- Create `IConflictDetectionService` interface
- Implement conflict detection rules per spec:
  - Same subject + predicate + overlapping time = potential conflict
  - Check object compatibility:
    - `is_a` type conflicts
    - `birthday_day_month` date conflicts
    - `attribute` value conflicts (with tolerance)
    - Relationship conflicts (rare)
- Calculate conflict severity (info/warn/error)
- Create `QuestionableItem` records for detected conflicts
- Include both assertion IDs in related_assertions
- Store conflict details in structured JSON

**Acceptance Criteria**:
- [ ] Detects type conflicts (is_a)
- [ ] Detects date conflicts (birthday)
- [ ] Detects attribute value conflicts
- [ ] Correctly handles time scope overlaps
- [ ] Creates appropriate questionable items
- [ ] Severity calculated correctly
- [ ] Unit tests for each conflict type
- [ ] Integration test with realistic data

**Files to Create**:
- `src/Palimpsest.Application/Interfaces/Services/IConflictDetectionService.cs` (new)
- `src/Palimpsest.Infrastructure/Services/ConflictDetectionService.cs` (new)

---

### Task 5.2: Constraint Validation ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Medium  
**Dependencies**: Task 5.1  
**Estimated Effort**: 4-6 hours

**Requirements**:
- Implement basic constraint checks:
  - Parent-child relationship transitivity
  - Age constraints (child can't be parent, etc.)
  - Location containment (entity can't be in two places simultaneously)
- Create questionable items for violations
- Support configurable constraint rules
- Flag low-confidence assertions (< threshold)

**Acceptance Criteria**:
- [ ] Detects parent-child constraint violations
- [ ] Detects temporal/spatial impossibilities
- [ ] Flags low-confidence assertions
- [ ] Constraint rules configurable
- [ ] Unit tests for each constraint type

**Files to Create/Modify**:
- `src/Palimpsest.Infrastructure/Services/ConstraintValidationService.cs` (new)
- `src/Palimpsest.Infrastructure/Services/ConflictDetectionService.cs` (enhance)

---

### Task 5.3: Identity Ambiguity Detection ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Medium  
**Dependencies**: Phase 2 complete  
**Estimated Effort**: 3-5 hours

**Requirements**:
- Detect when multiple entities might refer to same real-world entity
- Check for:
  - Very similar canonical names
  - Overlapping alias sets
  - Shared attributes/relationships suggesting same entity
- Create questionable items for potential merges
- Store similarity evidence in details JSON

**Acceptance Criteria**:
- [ ] Detects potential duplicate entities
- [ ] Similarity scoring is reasonable
- [ ] Creates questionable items with merge suggestions
- [ ] Evidence included for author review
- [ ] Unit tests with known duplicates

**Files to Create**:
- `src/Palimpsest.Infrastructure/Services/IdentityAmbiguityService.cs` (new)

---

### Task 5.4: Integrate Reconciliation into Pipeline ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: Medium  
**Dependencies**: Task 5.1, Task 5.2, Task 5.3  
**Estimated Effort**: 3-4 hours

**Requirements**:
- Update `IngestionService.IngestDocumentAsync()` to run reconciliation
- Run conflict detection after assertion extraction completes
- Run constraint validation
- Run identity ambiguity detection
- Mark affected entities as dirty for dossier rebuild
- Update job progress JSON

**Acceptance Criteria**:
- [ ] Reconciliation runs automatically after extraction
- [ ] Questionable items created for all detected issues
- [ ] Entities marked dirty appropriately
- [ ] Job tracks reconciliation stage
- [ ] Pipeline completes successfully end-to-end

**Files to Modify**:
- `src/Palimpsest.Infrastructure/Services/IngestionService.cs`

---

### Task 5.5: Trigger Dossier Rebuild ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Low  
**Dependencies**: Task 5.4, Phase 3 complete  
**Estimated Effort**: 2-3 hours

**Requirements**:
- After reconciliation, queue dossier rebuild job for dirty entities
- Support immediate or deferred rebuild
- Batch dirty entities efficiently
- Clear dirty flags after successful rebuild

**Acceptance Criteria**:
- [ ] Dossier rebuild triggered automatically
- [ ] Only dirty entities processed
- [ ] Batching works efficiently
- [ ] Job system handles rebuild jobs

**Files to Modify**:
- `src/Palimpsest.Infrastructure/Services/IngestionService.cs`
- Job processing service

---

## Phase 6: UI Enhancements & Job Management

**Goal**: Provide visibility and control over the ingestion pipeline.

### Task 6.1: Job Status Dashboard ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Low-Medium  
**Dependencies**: None  
**Estimated Effort**: 4-5 hours

**Requirements**:
- Create Jobs controller and views
- Display all jobs with status, progress, timing
- Show detailed progress JSON in expandable view
- Support filtering by type, status, document
- Show error messages for failed jobs
- Add retry action for failed jobs
- Real-time updates (polling or SignalR)

**Acceptance Criteria**:
- [ ] Jobs list page shows all ingestion jobs
- [ ] Progress indicators visible
- [ ] Can view detailed job information
- [ ] Failed jobs show error messages
- [ ] Can retry failed jobs

**Files to Create**:
- `src/Palimpsest.Web/Controllers/JobsController.cs` (new)
- `src/Palimpsest.Web/Views/Jobs/Index.cshtml` (new)
- `src/Palimpsest.Web/Views/Jobs/Details.cshtml` (new)

---

### Task 6.2: Questionable Items Review UI ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Medium  
**Dependencies**: Phase 5 complete  
**Estimated Effort**: 6-8 hours

**Requirements**:
- Enhance QuestionableController (already exists)
- Show all questionable items by type and severity
- Display conflict details side-by-side
- Allow author to:
  - Mark canon (choose winning assertion)
  - Mark as retcon (override)
  - Dismiss (not a real conflict)
  - Merge entities (for identity conflicts)
- Update resolution status and capture notes
- Trigger dossier rebuild after resolution

**Acceptance Criteria**:
- [ ] Can view all questionable items
- [ ] Conflict details clearly presented
- [ ] Resolution actions work correctly
- [ ] Status updated in database
- [ ] Dossiers rebuilt after changes
- [ ] Resolution audit trail maintained

**Files to Modify**:
- `src/Palimpsest.Web/Controllers/QuestionableController.cs` (enhance)
- `src/Palimpsest.Web/Views/Questionable/*.cshtml` (enhance)

---

### Task 6.3: Entity Merge Functionality ⏳ NOT STARTED
**Priority**: LOW-MEDIUM  
**Complexity**: High  
**Dependencies**: Task 6.2  
**Estimated Effort**: 6-8 hours

**Requirements**:
- Create entity merge service
- When merging entities A → B:
  - Move all assertions from A to B
  - Move all aliases from A to B
  - Update all mentions pointing to A → point to B
  - Update all edges from/to A → point to B
  - Mark A as merged (soft delete or status)
  - Rebuild dossier for B
- Validate merge is safe (no new conflicts)
- Support undo/revert

**Acceptance Criteria**:
- [ ] Can merge two entities
- [ ] All related data updated correctly
- [ ] No data loss
- [ ] Dossier rebuilt for surviving entity
- [ ] Transaction rollback on errors
- [ ] Integration tests for merge scenarios

**Files to Create**:
- `src/Palimpsest.Infrastructure/Services/EntityMergeService.cs` (new)
- `src/Palimpsest.Application/Interfaces/Services/IEntityMergeService.cs` (new)

---

### Task 6.4: Document Processing Status ⏳ NOT STARTED
**Priority**: LOW  
**Complexity**: Low  
**Dependencies**: Phase 4 complete  
**Estimated Effort**: 2-3 hours

**Requirements**:
- Add processing status badge to Documents/Index
- Show in Documents/Details:
  - Segments count
  - Entities extracted
  - Assertions extracted
  - Questionable items count
  - Processing job status
- Add "Reprocess Document" action

**Acceptance Criteria**:
- [ ] Document status visible in list and details
- [ ] Statistics accurate
- [ ] Can trigger reprocessing
- [ ] Status updates in real-time (or on refresh)

**Files to Modify**:
- `src/Palimpsest.Web/Views/Documents/Index.cshtml`
- `src/Palimpsest.Web/Views/Documents/Details.cshtml`
- `src/Palimpsest.Web/Controllers/DocumentsController.cs`

---

## Phase 7: Performance & Polish

**Goal**: Optimize performance and improve user experience.

### Task 7.1: Background Job Processing ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: Medium-High  
**Dependencies**: Phase 4 complete  
**Estimated Effort**: 6-8 hours

**Requirements**:
- Implement background job queue (Hangfire or custom)
- Move LLM processing to background workers
- Support concurrent job processing (configurable parallelism)
- Implement job retry logic with exponential backoff
- Add job cancellation support
- Monitor queue health and worker status

**Acceptance Criteria**:
- [ ] Jobs process in background, not blocking UI
- [ ] Multiple jobs can process concurrently
- [ ] Failed jobs retry automatically
- [ ] Can cancel running jobs
- [ ] Worker health monitoring
- [ ] Admin dashboard for job queue

**Files to Create/Modify**:
- Add Hangfire or similar library
- `src/Palimpsest.Infrastructure/Jobs/IngestionJobProcessor.cs` (new)
- `src/Palimpsest.Web/Program.cs` (configure background jobs)

---

### Task 7.2: Segment Embedding Generation ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Medium  
**Dependencies**: Phase 4 complete  
**Estimated Effort**: 4-5 hours

**Requirements**:
- Integrate embedding service (already interface exists)
- Generate embeddings for each segment after creation
- Use OpenAI text-embedding-ada-002 or similar
- Store in segment_embeddings table with pgvector
- Support batch embedding generation
- Add to ingestion pipeline

**Acceptance Criteria**:
- [ ] Embeddings generated for all segments
- [ ] Stored in database with pgvector
- [ ] Batch processing for efficiency
- [ ] Part of standard ingestion flow
- [ ] Vector similarity search works

**Files to Modify**:
- `src/Palimpsest.Infrastructure/Services/EmbeddingService.cs` (implement)
- `src/Palimpsest.Infrastructure/Services/IngestionService.cs` (integrate)

---

### Task 7.3: Semantic Search ⏳ NOT STARTED
**Priority**: LOW-MEDIUM  
**Complexity**: Medium  
**Dependencies**: Task 7.2  
**Estimated Effort**: 5-6 hours

**Requirements**:
- Implement semantic search over segments using embeddings
- Support hybrid search (semantic + lexical)
- Add search API endpoint
- Create search UI
- Return segments with context (adjacent segments)
- Highlight entity mentions in results

**Acceptance Criteria**:
- [ ] Semantic search returns relevant segments
- [ ] Hybrid search combines vector + keyword
- [ ] Results include context
- [ ] UI responsive and useful
- [ ] Search within universe scope

**Files to Create**:
- `src/Palimpsest.Web/Controllers/SearchController.cs` (new)
- `src/Palimpsest.Web/Views/Search/Index.cshtml` (new)
- Search service implementation

---

### Task 7.4: Performance Optimization ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Medium  
**Dependencies**: Most features complete  
**Estimated Effort**: 6-8 hours

**Requirements**:
- Add database indexes for common queries
- Implement caching for dossiers (Redis or in-memory)
- Optimize N+1 queries (use .Include() appropriately)
- Add pagination to large lists
- Lazy load relationships where appropriate
- Profile and optimize slow queries

**Acceptance Criteria**:
- [ ] Key pages load in < 500ms
- [ ] No N+1 query issues
- [ ] Dossiers cached effectively
- [ ] Pagination on all large lists
- [ ] Database query plan analysis complete

**Files to Review/Modify**:
- All repository implementations
- All controller actions
- Database indexes

---

### Task 7.5: Error Handling & Logging ⏳ NOT STARTED
**Priority**: MEDIUM  
**Complexity**: Low-Medium  
**Dependencies**: None  
**Estimated Effort**: 3-4 hours

**Requirements**:
- Standardize error handling across services
- Add structured logging (Serilog)
- Log LLM requests/responses (with PII filtering)
- Add error telemetry (Application Insights or similar)
- Create error pages with helpful messages
- Log job failures with full context

**Acceptance Criteria**:
- [ ] All errors logged with context
- [ ] User-friendly error messages
- [ ] No sensitive data in logs
- [ ] Structured logging queryable
- [ ] Error monitoring dashboard

**Files to Modify**:
- `src/Palimpsest.Web/Program.cs` (configure logging)
- All service classes (add logging)

---

## Phase 8: Testing & Documentation

### Task 8.1: Integration Test Suite ⏳ NOT STARTED
**Priority**: HIGH  
**Complexity**: Medium-High  
**Dependencies**: Most features complete  
**Estimated Effort**: 8-10 hours

**Requirements**:
- End-to-end test: document upload → extraction → conflicts
- Test with realistic sample documents
- Test error scenarios (LLM failures, malformed data)
- Test entity resolution edge cases
- Test conflict detection accuracy
- Test dossier generation
- Mock LLM responses for consistent testing

**Acceptance Criteria**:
- [ ] Full pipeline test passes
- [ ] Edge cases covered
- [ ] Error scenarios handled gracefully
- [ ] Tests run in CI/CD
- [ ] Code coverage > 70%

**Files to Create**:
- `src/Palimpsest.Tests.Integration/Pipeline/FullIngestionTests.cs` (new)
- Sample test documents in test resources

---

### Task 8.2: API Documentation ⏳ NOT STARTED
**Priority**: LOW  
**Complexity**: Low  
**Dependencies**: None  
**Estimated Effort**: 3-4 hours

**Requirements**:
- Add XML documentation comments to all public APIs
- Generate API documentation (Swagger/OpenAPI)
- Document service interfaces
- Document repository patterns
- Create architecture decision records (ADRs)

**Acceptance Criteria**:
- [ ] All public APIs documented
- [ ] Swagger UI available
- [ ] Architecture documented
- [ ] Decision rationale captured

**Files to Modify**:
- All interface and public class files
- Add ADR documents

---

### Task 8.3: User Guide ⏳ NOT STARTED
**Priority**: LOW  
**Complexity**: Low  
**Dependencies**: UI complete  
**Estimated Effort**: 4-5 hours

**Requirements**:
- Create user guide for document ingestion
- Document entity management workflow
- Document conflict resolution process
- Create troubleshooting guide
- Add in-app help/tooltips
- Create video walkthroughs (optional)

**Acceptance Criteria**:
- [ ] User guide covers all major workflows
- [ ] Screenshots included
- [ ] Troubleshooting section complete
- [ ] Help accessible from UI

**Files to Create**:
- `USER_GUIDE.md` (new)
- Help content in Views

---

## Dependencies & Critical Path

### Critical Path (Must complete in order):
1. **Phase 2** (Entity Detection) → Must complete before Phase 3 or 4
2. **Phase 3** (Dossiers) → Must complete before Phase 4
3. **Phase 4** (Assertion Extraction) → Must complete before Phase 5
4. **Phase 5** (Conflict Detection) → Completes core functionality
5. **Phase 6** (UI) → Can run in parallel with testing
6. **Phase 7** (Performance) → After most features done
7. **Phase 8** (Testing) → Throughout and at end

### Parallelizable Work:
- Task 4.1 (LLM Interface) can start immediately
- Task 6.1 (Job Dashboard) can start any time
- Task 7.5 (Logging) can run throughout
- Task 8.2 (Documentation) can run throughout

---

## Effort Estimates

| Phase | Tasks | Total Hours | Agent Days (6hr) |
|-------|-------|-------------|------------------|
| Phase 2 | 4 tasks | 14-20 hrs | 2-3 days |
| Phase 3 | 3 tasks | 11-15 hrs | 2-3 days |
| Phase 4 | 4 tasks | 18-24 hrs | 3-4 days |
| Phase 5 | 5 tasks | 20-28 hrs | 3-5 days |
| Phase 6 | 4 tasks | 18-26 hrs | 3-4 days |
| Phase 7 | 5 tasks | 24-32 hrs | 4-5 days |
| Phase 8 | 3 tasks | 15-19 hrs | 2-3 days |
| **TOTAL** | **28 tasks** | **120-164 hrs** | **20-27 agent days** |

---

## Getting Started

### For Next Agent:
1. **Start with Phase 2, Task 2.1** (Mention Detection Service)
2. Read the spec sections on mention detection (Section 6.2, Stage B)
3. Review existing `IngestionService` to understand current pipeline
4. Create the `IEntityMentionService` interface first
5. Implement basic capitalization-based mention detection
6. Write unit tests as you go

### Prerequisites:
- ✅ Database schema complete
- ✅ Entity models defined
- ✅ Basic repositories implemented
- ✅ Segmentation working

### Configuration Needed:
- LLM API keys (OpenAI, Anthropic, or Azure OpenAI)
- Configure in `appsettings.json` or user secrets
- Set up rate limits and quotas

---

## Success Criteria

The ingestion pipeline is complete when:

1. ✅ A document can be uploaded with text content
2. ✅ Text is segmented intelligently (chapter-aware)
3. ✅ Entities are detected and resolved from text
4. ✅ Assertions are extracted with LLM
5. ✅ Conflicts are detected automatically
6. ✅ Questionable items appear in review queue
7. ✅ Author can resolve conflicts via UI
8. ✅ Dossiers are generated and kept current
9. ✅ Graph relationships (edges) are built
10. ✅ All processing happens in background
11. ✅ Performance is acceptable (< 30sec per 1000 words)
12. ✅ Error handling is robust
13. ✅ Tests pass consistently

---

## Notes for Future Agents

- **Follow the spec**: The `spec` file is authoritative
- **Test as you go**: Don't wait until end for testing
- **Commit frequently**: Small, logical commits with clear messages
- **Update this tracker**: Mark tasks complete, add discovered subtasks
- **Ask questions**: If spec is unclear, document assumptions
- **Performance matters**: This will process large documents (100k+ words)
- **LLM costs**: Be mindful of token usage and costs
- **Error handling**: LLMs fail, networks fail, handle gracefully

---

## Change Log

| Date | Agent | Change |
|------|-------|--------|
| 2026-01-05 | Initial | Created tracker with all phases |

