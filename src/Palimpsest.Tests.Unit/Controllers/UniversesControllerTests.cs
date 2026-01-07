using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Domain.Entities;
using Palimpsest.Web.Controllers;

namespace Palimpsest.Tests.Unit.Controllers;

public class UniversesControllerTests
{
    private readonly Mock<IUniverseRepository> _mockRepository;
    private readonly Mock<IUniverseContextService> _mockContextService;
    private readonly Mock<ILogger<UniversesController>> _mockLogger;
    private readonly UniversesController _controller;

    public UniversesControllerTests()
    {
        _mockRepository = new Mock<IUniverseRepository>();
        _mockContextService = new Mock<IUniverseContextService>();
        _mockLogger = new Mock<ILogger<UniversesController>>();
        
        _controller = new UniversesController(
            _mockRepository.Object,
            _mockContextService.Object,
            _mockLogger.Object);

        // Setup TempData for the controller
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithUniverses()
    {
        // Arrange
        var universes = new List<Universe>
        {
            new Universe { UniverseId = Guid.NewGuid(), Name = "Universe 1" },
            new Universe { UniverseId = Guid.NewGuid(), Name = "Universe 2" }
        };
        _mockRepository.Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(universes);
        _mockContextService.Setup(s => s.GetActiveUniverseId())
            .Returns(universes[0].UniverseId);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Universe>>().Subject;
        model.Should().HaveCount(2);
        viewResult.ViewData["ActiveUniverseId"].Should().Be(universes[0].UniverseId);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_ValidUniverse_CreatesAndRedirects()
    {
        // Arrange
        var universe = new Universe
        {
            Name = "Test Universe",
            AuthorName = "Test Author",
            Description = "Test Description"
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Universe>(), default))
            .ReturnsAsync(universe);

        // Act
        var result = await _controller.Create(universe);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UniversesController.Index));
        
        _mockRepository.Verify(r => r.CreateAsync(
            It.Is<Universe>(u => u.Name == "Test Universe"),
            default), Times.Once);
        
        _mockContextService.Verify(s => s.SetActiveUniverseId(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        var universe = new Universe { Name = "Test" };
        _controller.ModelState.AddModelError("Description", "Required");

        // Act
        var result = await _controller.Create(universe);

        // Assert
        result.Should().BeOfType<ViewResult>();
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Universe>(), default), Times.Never);
    }

    [Fact]
    public async Task Details_ExistingUniverse_ReturnsView()
    {
        // Arrange
        var universeId = Guid.NewGuid();
        var universe = new Universe
        {
            UniverseId = universeId,
            Name = "Test Universe"
        };
        _mockRepository.Setup(r => r.GetByIdAsync(universeId, default))
            .ReturnsAsync(universe);
        _mockContextService.Setup(s => s.GetActiveUniverseId())
            .Returns(universeId);

        // Act
        var result = await _controller.Details(universeId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Universe>().Subject;
        model.UniverseId.Should().Be(universeId);
        viewResult.ViewData["ActiveUniverseId"].Should().Be(universeId);
    }

    [Fact]
    public async Task Details_NonExistentUniverse_ReturnsNotFound()
    {
        // Arrange
        var universeId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(universeId, default))
            .ReturnsAsync((Universe?)null);

        // Act
        var result = await _controller.Details(universeId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void SetActive_ValidId_SetsActiveAndRedirects()
    {
        // Arrange
        var universeId = Guid.NewGuid();

        // Act
        var result = _controller.SetActive(universeId);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UniversesController.Index));
        
        _mockContextService.Verify(s => s.SetActiveUniverseId(universeId), Times.Once);
    }
}
