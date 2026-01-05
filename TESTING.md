# Testing Strategy for Palimpsest

## Unit Tests (Palimpsest.Tests.Unit)

Unit tests use **Moq** to mock dependencies and test business logic in isolation without requiring a database.

### Current Coverage
- ✅ **UniversesControllerTests** (7 tests) - Tests controller actions with mocked repositories
- ✅ **UniverseContextServiceTests** (4 tests) - Tests session management

### Why Not InMemory Database?
The InMemory provider doesn't support pgvector extensions, which our schema requires. Instead:
- **Controllers**: Mock the repository interfaces
- **Services**: Mock HttpContext and dependencies
- **Pure logic**: Test directly without database

### Running Unit Tests
```bash
cd src/Palimpsest.Tests.Unit
dotnet test
```

### Example Unit Test Pattern
```csharp
public class MyControllerTests
{
    private readonly Mock<IMyRepository> _mockRepository;
    private readonly MyController _controller;

    public MyControllerTests()
    {
        _mockRepository = new Mock<IMyRepository>();
        _controller = new MyController(_mockRepository.Object);
    }

    [Fact]
    public async Task Action_Scenario_ExpectedResult()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new MyEntity());

        // Act
        var result = await _controller.Action();

        // Assert
        result.Should().BeOfType<OkResult>();
        _mockRepository.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Once);
    }
}
```

## Integration Tests (Palimpsest.Tests.Integration)

Integration tests use **Testcontainers** to spin up a real PostgreSQL database with pgvector for testing the full stack.

### Setup Required
The integration tests will automatically:
1. Start a PostgreSQL container with pgvector
2. Apply migrations
3. Run tests against the real database
4. Clean up the container

### Running Integration Tests
```bash
cd src/Palimpsest.Tests.Integration
dotnet test
```

### Using Testcontainers (Future Enhancement)

To implement repository tests with a real database:

```csharp
public class UniverseRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres;
    private PalimpsestDbContext _context;
    private UniverseRepository _repository;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg16")
            .Build();
        
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<PalimpsestDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new PalimpsestDbContext(options);
        await _context.Database.MigrateAsync();
        
        _repository = new UniverseRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ValidUniverse_CreatesInDatabase()
    {
        // Arrange
        var universe = new Universe { Name = "Test", CreatedAt = DateTime.UtcNow };

        // Act
        await _repository.CreateAsync(universe);

        // Assert
        var retrieved = await _repository.GetByIdAsync(universe.UniverseId);
        retrieved.Should().NotBeNull();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
```

## Test Organization

```
Palimpsest.Tests.Unit/
├── Controllers/           # Controller tests with mocked services
├── Services/              # Service tests with mocked dependencies
└── ...

Palimpsest.Tests.Integration/
├── Controllers/           # Full HTTP integration tests
├── Repositories/          # Database integration tests (future)
└── PalimpsestWebApplicationFactory.cs
```

## Best Practices

### Unit Tests
- ✅ Mock all dependencies
- ✅ Test one unit of work
- ✅ Fast execution (< 1ms per test)
- ✅ No external dependencies

### Integration Tests
- ✅ Test real database interactions
- ✅ Test full HTTP request/response cycle
- ✅ Verify end-to-end behavior
- ⚠️ Slower (may take seconds)

## CI/CD Considerations

For CI pipelines:
- Unit tests: Run on every commit (fast feedback)
- Integration tests: Run on PR and before merge (thorough validation)
- Docker required for integration tests with Testcontainers

## Future Enhancements

1. Add repository integration tests with Testcontainers
2. Add performance tests for large document ingestion
3. Add mutation testing to verify test quality
4. Set up code coverage reporting (target: 80%+)
