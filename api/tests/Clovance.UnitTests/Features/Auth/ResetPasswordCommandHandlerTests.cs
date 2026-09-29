using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Auth.ResetPassword;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Clovance.UnitTests.Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class ResetPasswordCommandHandlerTests : IAsyncLifetime
{
    private readonly ClovanceDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _handler = new ResetPasswordCommandHandler(_userManager, _dbContext);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _dbContext.DisposeAsync();

    [Fact]
    public async Task HandleAsync_WhenUserExistsAndTokenIsValid_ResetsPasswordAndRemovesActiveRefreshTokens()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "user@example.com",
            UserName = "user@example.com"
        };

        var command = new ResetPasswordCommand(user.Email, "valid-token", "NewPassword123!");

        var activeTokenForUser = RefreshToken.Create(user.Id, TestData.TokenHash, DateTimeOffset.UtcNow.AddDays(1));
        var usedTokenForUser = RefreshToken.Create(user.Id, "A3F1C9E2B8D47A6F0E5C3B9A1D8F4E7C2A6B9D3F8E1C4A7B0D5F2E8C1A4B7D9F", DateTimeOffset.UtcNow.AddDays(1));
        usedTokenForUser.MarkAsUsed();

        var activeTokenForOtherUser = RefreshToken.Create(Guid.NewGuid(), "A3F1C9E2B8D47A6F0E5C3B9A1D8F4E7C2A6B9D3F8E1C4A7B0D5F2E8C1A4B7D9E", DateTimeOffset.UtcNow.AddDays(1));

        await _dbContext.RefreshTokens.AddRangeAsync(new[] { activeTokenForUser, usedTokenForUser, activeTokenForOtherUser }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword).Returns(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        var remainingTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == RefreshTokenUserId.Create(user.Id))
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.DoesNotContain(remainingTokens, t => !t.IsUsed);
        Assert.Contains(remainingTokens, t => t.IsUsed);

        var otherUserTokenCount = await _dbContext.RefreshTokens
            .CountAsync(t => t.UserId == RefreshTokenUserId.Create(activeTokenForOtherUser.UserId.Value), TestContext.Current.CancellationToken);

        Assert.Equal(1, otherUserTokenCount);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsInvalidTokenError()
    {
        // Arrange
        var command = new ResetPasswordCommand("missing@example.com", "token", "NewPassword123!");
        _userManager.FindByEmailAsync(command.Email).Returns((ApplicationUser?)null);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.PasswordResetInvalidOrExpiredResetToken, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsInvalid_ReturnsInvalidTokenError()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "user@example.com",
            UserName = "user@example.com"
        };

        var command = new ResetPasswordCommand(user.Email, "invalid-token", "NewPassword123!");

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword)
            .Returns(IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidToken",
                Description = "Invalid token."
            }));

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.PasswordResetInvalidOrExpiredResetToken, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenResetFailsForAnotherReason_ReturnsPasswordResetFailedError()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "user@example.com",
            UserName = "user@example.com"
        };

        var command = new ResetPasswordCommand(user.Email, "valid-token", "NewPassword123!");

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword)
            .Returns(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = "Password is too short."
            }));

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.PasswordChangeFailed, result.Error?.Code);
        Assert.Contains("Password is too short", result.Error?.Description);
    }
}
