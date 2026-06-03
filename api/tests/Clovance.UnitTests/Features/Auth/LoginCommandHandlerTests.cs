using Clovance.ApiService.Features.Auth.Login;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _jwtTokenService = Substitute.For<IJwtTokenService>();

        _handler = new LoginCommandHandler(_userManager, _jwtTokenService);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        var command = new LoginCommand("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com",
            MustCompleteOnboarding = false
        };
        var roles = new List<string> { "User" };
        var expectedToken = "jwt-token";
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(true);
        _userManager.GetRolesAsync(user).Returns(roles);
        _jwtTokenService.GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<bool>())
            .Returns((expectedToken, expectedExpiresAt));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value.AccessToken);
        Assert.Equal(expectedExpiresAt, result.Value.ExpiresAt);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ReturnsInvalidCredentialsError()
    {
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        _userManager.FindByEmailAsync(command.Email).Returns((ApplicationUser?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvalidCredentials, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPassword_ReturnsInvalidCredentialsError()
    {
        var command = new LoginCommand("test@example.com", "WrongPassword");
        var user = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(false);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvalidCredentials, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithMustCompleteOnboarding_IncludesInToken()
    {
        var command = new LoginCommand("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com",
            MustCompleteOnboarding = true
        };
        var roles = new List<string> { "User" };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(true);
        _userManager.GetRolesAsync(user).Returns(roles);
        _jwtTokenService.GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), true)
            .Returns(("token", DateTimeOffset.UtcNow.AddMinutes(60)));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _jwtTokenService.Received(1).GenerateToken(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            true);
    }
}
