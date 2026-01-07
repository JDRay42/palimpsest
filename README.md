[!NOTE]
This project is rooted in an idea for experimenting with AI memory structures. It is almost entirely "vibe coded" (though I dislike that term). On the surface, it is an app for authors to manage fictional universes, but underneath it is an exploration of knowledge representation, epistemology, and human-AI collaboration. Or so that's my intent.

# Palimpsest

A canon-aware knowledge engine for authors managing large fictional universes across many books and timelines. Track every character, event, and assertion with full provenance. Never lose track of what your characters know, when they learned it, or which version of your manuscript said it first.

## Overview

Palimpsest is a local-first authoring tool designed to help authors maintain consistency across complex fictional universes. It extracts and stores entities and assertions with provenance, supports progressive enrichment, handles arbitrary ingestion order, and flags questionable assertions into a review queue.

### Key Features

- **Graph-First Architecture**: Entities, Assertions, Edges, and Conflicts stored in PostgreSQL
- **Append-Only Canon**: New knowledge is added; conflicts are flagged, not auto-resolved
- **Epistemic Awareness**: Every assertion is explicitly Observed, Believed, or Inferred
- **Mandatory Provenance**: Every assertion points to evidence segments with stable locators
- **Universe-Scoped Operations**: All chat, ingestion, search, and commands operate within an active universe

## Architecture

Palimpsest follows **Clean Architecture** principles:

- **Domain Layer** (`Palimpsest.Domain`): Core entities, enums, value objects - no external dependencies
- **Application Layer** (`Palimpsest.Application`): Use cases, repository interfaces, service interfaces
- **Infrastructure Layer** (`Palimpsest.Infrastructure`): Database access, EF Core, LLM providers, external services
- **Presentation Layer** (`Palimpsest.Web`): ASP.NET Core web application with HTMX and Shoelace

## Technology Stack

- **Framework**: .NET 8.0 (LTS)
- **Frontend**: Server-rendered HTML with HTMX and Shoelace web components
- **Database**: PostgreSQL with pgvector and pg_trgm extensions
- **ORM**: Entity Framework Core
- **Deployment**: Docker Compose (local-first)
- **LLM Integration**: Pluggable provider architecture (Ollama, OpenRouter)

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker and Docker Compose](https://docs.docker.com/get-docker/)
- Git

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/JDRay42/palimpsest.git
   cd palimpsest
   ```

2. **Start PostgreSQL with pgvector**

   ```bash
   docker-compose up -d
   ```

   This will start a PostgreSQL instance with pgvector and pg_trgm extensions enabled.

3. **Run database migrations**

   ```bash
   cd src/Palimpsest.Infrastructure
   dotnet ef database update
   ```

4. **Run the application**

   ```bash
   cd ../Palimpsest.Web
   dotnet run
   ```

5. **Open your browser**

   Navigate to `https://localhost:7131` or `http://localhost:7130`

### Configuration

The application uses `appsettings.json` for configuration. Key settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=palimpsest;Username=palimpsest;Password=palimpsest_dev"
  }
}
```

For development, you can override settings in `appsettings.Development.json`.

## Usage

### Creating a Universe

1. Navigate to the Universes page
2. Click "Create Universe"
3. Enter a name, optional author name, and description
4. Click "Create Universe"
5. The newly created universe will automatically become active

### Managing Entities

**Pre-Loading Character Lists (Before Ingestion)**:

1. Navigate to Entities → Import Entities
2. Prepare your character list in JSON or CSV format:
   
   **JSON Format:**
   ```json
   [
     {
       "name": "Celestina Maria Foscari",
       "type": "Person",
       "aliases": ["Celeste", "The Lady Foscari", "Maria"]
     },
     {
       "name": "Venice",
       "type": "Place",
       "aliases": ["La Serenissima", "The Republic"]
     }
   ]
   ```
   
   **CSV Format:**
   ```csv
   name,type,aliases
   "Celestina Maria Foscari","Person","Celeste;The Lady Foscari;Maria"
   "Venice","Place","La Serenissima;The Republic"
   ```

3. Upload the file → Entities created with aliases
4. Ingest documents → NER system recognizes pre-loaded entities

**Manual Entity Creation**:
- Navigate to Entities → Create Entity
- Enter canonical name, select type, add aliases
- System automatically normalizes aliases for matching

**Validating Detected Mentions**:
1. Navigate to Entities → Select Entity → Mentions Tab
2. Review auto-detected mentions with confidence scores
3. **Approve**: Confirm correct entity link
4. **Reject**: Unlink false positive
5. **Remap**: Search and link to different entity

**Resolving Ambiguous Matches**:
1. Navigate to Questionable Items
2. Filter by status (Open/Resolved/Dismissed)
3. For each item:
   - **Link to Existing**: Search and select correct entity
   - **Create New**: Generate new entity from mention
   - **Dismiss**: Mark as false positive

### Ingesting Documents

1. Navigate to Documents → Create Document
2. Enter title, author, optional tags
3. Upload text file or paste content
4. System automatically:
   - Normalizes text
   - Creates segments (paragraph-based)
   - Detects entity mentions
   - Resolves entities using fuzzy matching
   - Creates QuestionableItems for ambiguous matches
5. Monitor job progress on Documents page
6. Review and validate entity mentions

### Understanding the Data Model

#### Core Concepts

- **Universe**: Top-level boundary for all canon operations
- **Document**: A book, notes, outline, or appendix
- **Segment**: A chunk of text (chapter/section/paragraph) with stable locators
- **Entity**: A person, place, organization, object, concept, or event
- **Assertion**: Subject-Predicate-Object triple with epistemic category and provenance
- **Edge**: Denormalized entity-to-entity relationship for graph traversal
- **Dossier**: Materialized bounded context packet for an entity

#### Epistemic Categories

- **Observed**: Narrator-level or asserted as factual by the text
- **Believed**: A character believes/says it; may be wrong
- **Inferred**: Derived by the tool (explicitly tracked)

## Development

### Project Structure

```
palimpsest/
├── src/
│   ├── Palimpsest.Domain/         # Core domain entities and enums
│   ├── Palimpsest.Application/    # Use cases and interfaces
│   ├── Palimpsest.Infrastructure/ # Database, repositories, services
│   ├── Palimpsest.Web/            # ASP.NET Core web application
│   └── Palimpsest.sln             # Solution file
├── docs/                          # Documentation
└── docker-compose.yml             # PostgreSQL with pgvector
```

### Building the Solution

```bash
dotnet build
```

### Running Tests

The project includes comprehensive unit tests for Phase 2 NER functionality (49 passing tests).

Run all tests:
```bash
cd src
dotnet test
```

Run tests for a specific project:
```bash
dotnet test Palimpsest.Tests.Unit/Palimpsest.Tests.Unit.csproj
```

Run with detailed output:
```bash
dotnet test --verbosity normal
```

**Test Coverage:**
- EntityMentionService: 14 tests (detection, stopwords, confidence scoring)
- EntityResolutionService: 28 tests (exact match, fuzzy match, ambiguity, entity creation)
- EntityAliasRepository: 7 tests (exact match, fuzzy match, normalization)

**Note:** Integration tests are scaffolded but require a PostgreSQL instance with pgvector and pg_trgm extensions, as the in-memory database doesn't support these features.

### Database Migrations

To create a new migration:

```bash
cd src/Palimpsest.Infrastructure
dotnet ef migrations add <MigrationName> --output-dir Data/Migrations
```

To apply migrations:

```bash
dotnet ef database update
```

## Implementation Status

### ✅ Phase 1: Infrastructure & Foundation (Complete)
- ✅ Clean Architecture solution structure
- ✅ Complete database schema implementation
- ✅ EF Core with PostgreSQL, pgvector, and pg_trgm
- ✅ Repository pattern for all entities
- ✅ Universe context service
- ✅ Basic web UI with universe management
- ✅ Document creation and upload
- ✅ Text normalization and segmentation
- ✅ Job tracking with progress reporting

### ✅ Phase 2: Named Entity Recognition (Complete)
- ✅ Entity mention detection (capitalization-based with stopwords)
- ✅ Entity resolution with fuzzy matching (pg_trgm)
- ✅ QuestionableItem creation for ambiguous matches
- ✅ Entity CRUD operations with alias management
- ✅ Bulk import from JSON and CSV files
- ✅ Mention validation UI (approve/reject/remap)
- ✅ QuestionableItem resolution workflow
- ✅ **49 passing unit tests** covering all NER functionality

### ⏳ Phase 3: Dossier System (Planned)
- Bounded context packets for entities
- Materialized entity summaries with assertions and relationships
- Incremental rebuild on entity updates

### ⏳ Phase 4: Assertion Extraction (Planned)
- LLM integration (Ollama, OpenRouter)
- Subject-Predicate-Object triple extraction
- Epistemic category tagging (Observed/Believed/Inferred)
- Provenance linking to evidence segments

### ⏳ Phase 5: Conflict Detection (Planned)
- Assertion comparison and conflict identification
- QuestionableItem creation for contradictions
- Author review and reconciliation workflow

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please read the specification document (`spec`) before contributing to understand the project's design principles and architectural constraints.

## Acknowledgments

Built with Clean Architecture principles, following the specification defined in the `spec` document.
