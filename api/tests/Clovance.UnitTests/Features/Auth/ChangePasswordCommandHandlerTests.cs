using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Auth.ChangePassword;
using Clovance.ApiService.Infrastructure.Auth.Token;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Clovance.UnitTests.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class ChangePasswordCommandHandlerTests : AuthHandlerTestBase, IAsyncLifetime
{
    private const string NewTokenHash = "B4F1C9E2B8D47A6F0E5C3B9A1D8F4E7C2A6B9D3F8E1C4A7B0D5F2E8C1A4B7D9F";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;
    private readonly ClovanceDbContext _dbContext;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _userManager = CreateUserManager();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _tokenService = Substitute.For<ITokenService>();
        _tokenService.GenerateToken().Returns("new-refresh-token");
        _tokenService.HashToken("new-refresh-token").Returns(NewTokenHash);
        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        _handler = new ChangePasswordCommandHandler(_userManager, _httpContextAccessor, _tokenService, _dbContext);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _dbContext.DisposeAsync();

    [Fact]
    public async Task HandleAsync_WhenPasswordChangeSucceeds_RevokesActiveRefreshTokensAndIssuesNewOne()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.CreateVersion7() };
        var httpContext = CreateHttpContext(user.Id);
        var otherUserId = Guid.CreateVersion7();

        var activeTokenForUser = RefreshToken.Create(user.Id, TestData.TokenHash, DateTimeOffset.UtcNow.AddDays(1));
        var activeTokenForOtherUser = RefreshToken.Create(otherUserId, "A3F1C9E2B8D47A6F0E5C3B9A1D8F4E7C2A6B9D3F8E1C4A7B0D5F2E8C1A4B7D9E", DateTimeOffset.UtcNow.AddDays(1));

        await _dbContext.RefreshTokens.AddRangeAsync(new[] { activeTokenForUser, activeTokenForOtherUser }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        var userTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == RefreshTokenUserId.Create(user.Id))
            .ToListAsync(TestContext.Current.CancellationToken);

        var remaining = Assert.Single(userTokens);
        Assert.Equal(RefreshTokenTokenHash.Create(NewTokenHash), remaining.TokenHash);
        Assert.False(remaining.IsUsed);

        var otherUserTokenCount = await _dbContext.RefreshTokens
            .CountAsync(t => t.UserId == RefreshTokenUserId.Create(otherUserId), TestContext.Current.CancellationToken);
        Assert.Equal(1, otherUserTokenCount);

        Assert.Contains("clovance-refresh-token=new-refresh-token", httpContext.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordChangeFails_DoesNotRevokeRefreshTokens()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.CreateVersion7() };
        var httpContext = CreateHttpContext(user.Id);

        await _dbContext.RefreshTokens.AddAsync(
            RefreshToken.Create(user.Id, TestData.TokenHash, DateTimeOffset.UtcNow.AddDays(1)),
            TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new ChangePasswordCommand("WrongPassword123!", "NewPassword123!");

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        var token = await _dbContext.RefreshTokens.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(RefreshTokenTokenHash.Create(TestData.TokenHash), token.TokenHash);
        Assert.False(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotFound_ReturnsUserNotFoundError()
    {
        // Arrange
        var httpContext = CreateHttpContext(Guid.NewGuid());

        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns((ApplicationUser?)null);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotFound, result.Error?.Code);
        await _userManager.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordChangeFails_ReturnsPasswordChangeFailedError()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.CreateVersion7() };
        var httpContext = CreateHttpContext(user.Id);

        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Password is too short." });

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns(user);
        
        _userManager
            .ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(failedResult);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.PasswordChangeFailed, result.Error?.Code);
        Assert.Contains("Password is too short", result.Error?.Description);
    }

    [Fact]
    public async Task HandleAsync_WhenHttpContextIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command, TestContext.Current.CancellationToken));
    }
}
