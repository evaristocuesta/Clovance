using System.Security.Claims;
using Clovance.ApiService.Features.Auth.ChangePassword;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class ChangePasswordCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _handler = new ChangePasswordCommandHandler(_userManager, _httpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExistsAndPasswordChangeSucceeds_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.CreateVersion7() };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "TestAuthType"))
        };

        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        await _userManager.Received(1).ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotFound_ReturnsUserNotFoundError()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }, "TestAuthType"))
        };

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
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "TestAuthType"))
        };

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
