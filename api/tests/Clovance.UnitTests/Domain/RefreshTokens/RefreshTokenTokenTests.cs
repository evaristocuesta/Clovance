using Clovance.ApiService.Domain.RefreshTokens;

namespace Clovance.UnitTests.Domain.RefreshTokens;

public class RefreshTokenTokenTests
{
    [Fact]
    public void Create_WithValidValue_ShouldReturnRefreshTokenToken()
    {
        // Arrange
        var value = Guid.CreateVersion7().ToString();

        // Act
        var refreshTokenToken = RefreshTokenToken.Create(value);

        // Assert
        Assert.Equal(value, refreshTokenToken.Value);
    }

    [Fact]
    public void Create_WithNullValue_ShouldThrowArgumentException()
    {
        // Arrange
        string value = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenToken.Create(value));
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowArgumentException()
    {
        // Arrange
        var value = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenToken.Create(value));
    }
}
