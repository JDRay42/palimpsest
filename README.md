# Palimpsest

A canon-aware knowledge engine for authors managing large fictional universes across many books and timelines.

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

### Ingesting Documents

*Document ingestion UI coming in Phase 2*

For now, documents can be ingested programmatically using the `IIngestionService`.

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

*Test infrastructure coming soon*

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

## Phase 1 Status

Phase 1 focuses on infrastructure and foundational features:

- ✅ Clean Architecture solution structure
- ✅ Complete database schema implementation
- ✅ EF Core with PostgreSQL, pgvector, and pg_trgm
- ✅ Repository pattern for all entities
- ✅ Universe context service
- ✅ Ingestion service skeleton
- ✅ Stub LLM and embedding providers
- ✅ Basic web UI with universe management
- ⏳ Document upload UI (in progress)
- ⏳ Entity and assertion pipeline (stubbed)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please read the specification document (`spec`) before contributing to understand the project's design principles and architectural constraints.

## Acknowledgments

Built with Clean Architecture principles, following the specification defined in the `spec` document.
