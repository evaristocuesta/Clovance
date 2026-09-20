using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.UnitTests.Domain.Shared;

namespace Clovance.UnitTests.Domain.RefreshTokens;

public class RefreshTokenTokenHashTests
{
    [Fact]
    public void Create_WithValidValue_ShouldReturnRefreshTokenToken()
    {
        // Arrange
        var value = TestData.TokenHash;

        // Act
        var refreshTokenToken = RefreshTokenTokenHash.Create(value);

        // Assert
        Assert.Equal(value, refreshTokenToken.Value);
    }

    [Fact]
    public void Create_WithNullValue_ShouldThrowArgumentException()
    {
        // Arrange
        string value = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenTokenHash.Create(value));
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowArgumentException()
    {
        // Arrange
        var value = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshTokenTokenHash.Create(value));
    }
}
