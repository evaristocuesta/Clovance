using Clovance.ApiService.Domain.RefreshTokens;

namespace Clovance.UnitTests.Domain.RefreshTokens;

public class RefreshTokenUserIdTests
{
    [Fact]
    public void Create_WithValidValue_ShouldReturnRefreshTokenUserId()
    {
        // Arrange
        var value = Guid.CreateVersion7();

        // Act
        var refreshTokenUserId = RefreshTokenUserId.Create(value);

        // Assert
        Assert.Equal(value, refreshTokenUserId.Value);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenUserId.Create(value));
    }
}
