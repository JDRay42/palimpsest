using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Palimpsest.Infrastructure.Services;

namespace Palimpsest.Tests.Unit.Services;

public class UniverseContextServiceTests
{
    [Fact]
    public void GetActiveUniverseId_NoSessionValue_ReturnsNull()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockSession = new Mock<ISession>();
        var mockHttpContext = new Mock<HttpContext>();
        
        mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
        
        byte[] outValue;
        mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out outValue))
            .Returns(false);

        var service = new UniverseContextService(mockHttpContextAccessor.Object);

        // Act
        var result = service.GetActiveUniverseId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SetActiveUniverseId_ValidId_StoresInSession()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockSession = new Mock<ISession>();
        var mockHttpContext = new Mock<HttpContext>();
        
        mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new UniverseContextService(mockHttpContextAccessor.Object);

        // Act
        service.SetActiveUniverseId(testId);

        // Assert
        mockSession.Verify(s => s.Set(
            It.IsAny<string>(), 
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == testId.ToString())),
            Times.Once);
    }

    [Fact]
    public void RequireActiveUniverseId_NoActiveUniverse_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockSession = new Mock<ISession>();
        var mockHttpContext = new Mock<HttpContext>();
        
        mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
        
        byte[] outValue;
        mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out outValue))
            .Returns(false);

        var service = new UniverseContextService(mockHttpContextAccessor.Object);

        // Act
        Action act = () => service.RequireActiveUniverseId();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No active universe selected*");
    }

    [Fact]
    public void ClearActiveUniverse_RemovesFromSession()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockSession = new Mock<ISession>();
        var mockHttpContext = new Mock<HttpContext>();
        
        mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new UniverseContextService(mockHttpContextAccessor.Object);

        // Act
        service.ClearActiveUniverse();

        // Assert
        mockSession.Verify(s => s.Remove(It.IsAny<string>()), Times.Once);
    }
}
