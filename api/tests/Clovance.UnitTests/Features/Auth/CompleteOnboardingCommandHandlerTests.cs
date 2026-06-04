using System.Security.Claims;
using Clovance.ApiService.Features.Auth.CompleteOnboarding;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class CompleteOnboardingCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CompleteOnboardingCommandHandler _handler;
    private readonly HttpContext _httpContext;

    public CompleteOnboardingCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContext = Substitute.For<HttpContext>();

        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _handler = new CompleteOnboardingCommandHandler(
            _userManager,
            _httpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_CompletesOnboarding()
    {
        var command = new CompleteOnboardingCommand(
            "TempPassword123!",
            "NewPassword123!",
            "newemail@example.com");

        var userId = "user-123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "temp@example.com",
            UserName = "temp@example.com",
            MustCompleteOnboarding = true
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(userId);
        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);
        _userManager.FindByEmailAsync(command.NewEmail.Trim()).Returns((ApplicationUser?)null);
        _userManager.GenerateChangeEmailTokenAsync(user, command.NewEmail.Trim())
            .Returns("email-token");
        _userManager.ChangeEmailAsync(user, command.NewEmail.Trim(), "email-token")
            .Returns(IdentityResult.Success);
        _userManager.SetUserNameAsync(user, command.NewEmail.Trim())
            .Returns(IdentityResult.Success);
        _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.False(user.MustCompleteOnboarding);
        await _userManager.Received(1).ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        await _userManager.Received(1).ChangeEmailAsync(user, command.NewEmail.Trim(), "email-token");
        await _userManager.Received(1).SetUserNameAsync(user, command.NewEmail.Trim());
        await _userManager.Received(1).UpdateAsync(user);
    }

    [Fact]
    public async Task HandleAsync_WithoutAuthentication_ReturnsUserNotAuthenticatedError()
    {
        var command = new CompleteOnboardingCommand(
            "TempPassword123!",
            "NewPassword123!",
            "newemail@example.com");

        _httpContext.User.Returns(new ClaimsPrincipal());
        _userManager.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns((string?)null);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ReturnsUserNotFoundError()
    {
        var command = new CompleteOnboardingCommand(
            "TempPassword123!",
            "NewPassword123!",
            "newemail@example.com");

        var userId = "user-123";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(userId);
        _userManager.FindByIdAsync(userId).Returns((ApplicationUser?)null);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserNotFound, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCurrentPassword_ReturnsPasswordChangeFailedError()
    {
        var command = new CompleteOnboardingCommand(
            "WrongPassword",
            "NewPassword123!",
            "newemail@example.com");

        var userId = "user-123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "temp@example.com",
            MustCompleteOnboarding = true
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(userId);
        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.PasswordChangeFailed, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithEmailAlreadyInUse_ReturnsEmailAlreadyInUseError()
    {
        var command = new CompleteOnboardingCommand(
            "TempPassword123!",
            "NewPassword123!",
            "existing@example.com");

        var userId = "user-123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "temp@example.com",
            MustCompleteOnboarding = true
        };

        var existingUser = new ApplicationUser
        {
            Id = "other-user-456",
            Email = "existing@example.com"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(userId);
        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);
        _userManager.FindByEmailAsync(command.NewEmail.Trim()).Returns(existingUser);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.EmailAlreadyInUse, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithSameEmail_OnlyChangesPasswordAndCompletesOnboarding()
    {
        var command = new CompleteOnboardingCommand(
            "TempPassword123!",
            "NewPassword123!",
            "temp@example.com");

        var userId = "user-123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "temp@example.com",
            UserName = "temp@example.com",
            MustCompleteOnboarding = true
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(userId);
        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);
        _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.False(user.MustCompleteOnboarding);
        await _userManager.Received(1).ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        await _userManager.DidNotReceive().ChangeEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
        await _userManager.DidNotReceive().SetUserNameAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        await _userManager.Received(1).UpdateAsync(user);
    }

    [Fact]
    public async Task HandleAsync_WithEmailChangeFailed_ReturnsEmailChangeFailedError()
    {
        var command = new CompleteOnboardingCommand(
            "TempPassword123!",
            "NewPassword123!",
            "newemail@example.com");

        var userId = "user-123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "temp@example.com",
            MustCompleteOnboarding = true
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(userId);
        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);
        _userManager.FindByEmailAsync(command.NewEmail.Trim()).Returns((ApplicationUser?)null);
        _userManager.GenerateChangeEmailTokenAsync(user, command.NewEmail.Trim())
            .Returns("email-token");
        _userManager.ChangeEmailAsync(user, command.NewEmail.Trim(), "email-token")
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Email change failed" }));

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.EmailChangeFailed, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithUsernameChangeFailed_ReturnsUsernameChangeFailedError()
    {
        var command = new CompleteOnboardingCommand(
            "TempPassword123!",
            "NewPassword123!",
            "newemail@example.com");

        var userId = "user-123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "temp@example.com",
            MustCompleteOnboarding = true
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(userId);
        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword)
            .Returns(IdentityResult.Success);
        _userManager.FindByEmailAsync(command.NewEmail.Trim()).Returns((ApplicationUser?)null);
        _userManager.GenerateChangeEmailTokenAsync(user, command.NewEmail.Trim())
            .Returns("email-token");
        _userManager.ChangeEmailAsync(user, command.NewEmail.Trim(), "email-token")
            .Returns(IdentityResult.Success);
        _userManager.SetUserNameAsync(user, command.NewEmail.Trim())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Username change failed" }));

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UsernameChangeFailed, result.Error.Code);
    }
}
