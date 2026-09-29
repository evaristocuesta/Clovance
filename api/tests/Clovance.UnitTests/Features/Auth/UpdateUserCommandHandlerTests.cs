using Clovance.ApiService.Features.Auth.UpdateUser;
using Clovance.ApiService.Infrastructure.Auth.Jwt;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class UpdateUserCommandHandlerTests : AuthHandlerTestBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _userManager = CreateUserManager();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _handler = new UpdateUserCommandHandler(_userManager, _httpContextAccessor, _jwtTokenService);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExistsAndUpdateSucceeds_ReturnsUpdatedToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "old@example.com",
            UserName = "old@example.com",
            FirstName = "Old",
            LastName = "Name"
        };

        var httpContext = CreateHttpContext(user.Id);

        var command = new UpdateUserCommand("new@example.com", "New", "Surname");
        var generatedToken = (Token: "new-jwt-token", ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(20));

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns(user);
        _userManager.UpdateAsync(user).Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(user).Returns(new List<string> { "User" });
        _jwtTokenService.GenerateToken(user.Id, command.Email, Arg.Any<IEnumerable<string>>())
            .Returns(generatedToken);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(generatedToken.Token, result.Value.Token);
        Assert.Equal(command.Email, user.Email);
        Assert.Equal(command.Email, user.UserName);
        Assert.Equal(command.FirstName, user.FirstName);
        Assert.Equal(command.LastName, user.LastName);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotFound_ReturnsUserNotFoundError()
    {
        // Arrange
        var httpContext = CreateHttpContext(Guid.NewGuid());

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns((ApplicationUser?)null);

        // Act
        var result = await _handler.HandleAsync(new UpdateUserCommand("new@example.com", "New", "User"), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotFound, result.Error?.Code);
        await _userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ReturnsUserUpdateFailedError()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "old@example.com",
            UserName = "old@example.com"
        };

        var httpContext = CreateHttpContext(user.Id);

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns(user);
        _userManager.UpdateAsync(user).Returns(IdentityResult.Failed(new IdentityError { Description = "Update failed." }));

        // Act
        var result = await _handler.HandleAsync(new UpdateUserCommand("new@example.com", "New", "User"), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserUpdateFailed, result.Error?.Code);
    }
}
