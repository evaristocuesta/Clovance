using Clovance.ApiService.Domain.RefreshTokens;

namespace Clovance.UnitTests.Domain.RefreshTokens;

public class RefreshTokenUserIdTests
{
    [Fact]
    public void Create_WithValidValue_ShouldReturnRefreshTokenUserId()
    {
        // Arrange
        var value = Guid.NewGuid().ToString();

        // Act
        var refreshTokenUserId = RefreshTokenUserId.Create(value);

        // Assert
        Assert.Equal(value, refreshTokenUserId.Value);
    }

    [Fact]
    public void Create_WithNullValue_ShouldThrowArgumentException()
    {
        // Arrange
        string value = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenUserId.Create(value));
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowArgumentException()
    {
        // Arrange
        var value = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenUserId.Create(value));
    }

    [Fact]
    public void Create_WithInvalidGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = "invalid-guid";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenUserId.Create(value));
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty.ToString();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenUserId.Create(value));
    }
}
