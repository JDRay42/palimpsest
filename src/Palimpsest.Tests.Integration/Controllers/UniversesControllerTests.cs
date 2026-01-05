using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Tests.Integration.Controllers;

public class UniversesControllerTests : IClassFixture<PalimpsestWebApplicationFactory>
{
    private readonly PalimpsestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UniversesControllerTests(PalimpsestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Index_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/universes");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().Contain("text/html");
    }

    [Fact]
    public async Task Index_WithExistingUniverses_DisplaysUniverses()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PalimpsestDbContext>();
        
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "Integration Test Universe",
            AuthorName = "Test Author",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow
        };
        db.Universes.Add(universe);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/universes");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Contain("Integration Test Universe");
    }

    [Fact]
    public async Task Create_Get_ReturnsCreateForm()
    {
        // Act
        var response = await _client.GetAsync("/universes/create");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Contain("Create Universe");
        content.Should().Contain("name=\"Name\"");
    }

    [Fact]
    public async Task Create_Post_ValidData_CreatesUniverseAndRedirects()
    {
        // Arrange
        var getResponse = await _client.GetAsync("/universes/create");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = ExtractAntiForgeryToken(getContent);

        var formData = new Dictionary<string, string>
        {
            ["Name"] = "New Test Universe",
            ["AuthorName"] = "Integration Test Author",
            ["Description"] = "Created via integration test",
            ["__RequestVerificationToken"] = antiForgeryToken
        };

        var content = new FormUrlEncodedContent(formData);

        // Act
        var postResponse = await _client.PostAsync("/universes/create", content);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        postResponse.Headers.Location?.ToString().Should().Be("/universes");

        // Verify universe was created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PalimpsestDbContext>();
        var createdUniverse = db.Universes.FirstOrDefault(u => u.Name == "New Test Universe");
        
        createdUniverse.Should().NotBeNull();
        createdUniverse!.AuthorName.Should().Be("Integration Test Author");
    }

    [Fact]
    public async Task Details_ExistingUniverse_ReturnsDetailsPage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PalimpsestDbContext>();
        
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "Details Test Universe",
            AuthorName = "Test Author",
            CreatedAt = DateTime.UtcNow
        };
        db.Universes.Add(universe);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/universes/details/{universe.UniverseId}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Contain("Details Test Universe");
        content.Should().Contain("Test Author");
    }

    [Fact]
    public async Task Details_NonExistentUniverse_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/universes/details/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetActive_ValidUniverse_SetsActiveAndRedirects()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PalimpsestDbContext>();
        
        var universe = new Universe
        {
            UniverseId = Guid.NewGuid(),
            Name = "Active Test Universe",
            CreatedAt = DateTime.UtcNow
        };
        db.Universes.Add(universe);
        await db.SaveChangesAsync();

        var getResponse = await _client.GetAsync("/universes");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = ExtractAntiForgeryToken(getContent);

        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken
        };

        // Act
        var postResponse = await _client.PostAsync(
            $"/universes/setactive?id={universe.UniverseId}", 
            new FormUrlEncodedContent(formData));

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        postResponse.Headers.Location?.ToString().Should().Be("/universes");
    }

    private static string ExtractAntiForgeryToken(string htmlContent)
    {
        var match = Regex.Match(htmlContent, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]*)""");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Try alternate format
        match = Regex.Match(htmlContent, @"name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]*)""");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
