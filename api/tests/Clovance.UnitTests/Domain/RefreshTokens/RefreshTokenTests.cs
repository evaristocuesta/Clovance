using Clovance.ApiService.Domain.RefreshTokens;

namespace Clovance.UnitTests.Domain.RefreshTokens;

public class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldInitializePropertiesorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = "refreshToken123";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(48);

        // Act
        var refreshToken = RefreshToken.Create(userId, token, expiresAt);

        // Assert
        Assert.Equal(userId, refreshToken.UserId.Value);
        Assert.Equal(token, refreshToken.Token.Value);
        Assert.Equal(expiresAt, refreshToken.ExpiresAt);
        Assert.NotEqual(default, refreshToken.CreatedAt);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Arrange
        string userId = string.Empty;
        var token = "refreshToken123";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(48);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RefreshToken.Create(userId, token, expiresAt));
    }

    [Fact]
    public void MarkAsUsed_ShouldSetIsUsedToTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = "refreshToken123";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(48);
        var refreshToken = RefreshToken.Create(userId, token, expiresAt);

        // Act
        refreshToken.MarkAsUsed();

        // Assert
        Assert.True(refreshToken.IsUsed);
    }

    [Fact]
    public void MarkAsUsed_ShouldThrowException_WhenAlreadyUsed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = "refreshToken123";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(48);
        var refreshToken = RefreshToken.Create(userId, token, expiresAt);
        refreshToken.MarkAsUsed();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => refreshToken.MarkAsUsed());
    }
}
