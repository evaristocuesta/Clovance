using System.Security.Claims;
using Clovance.ApiService.Features.Auth.GetCurrentUser;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class GetCurrentUserQueryHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _handler = new GetCurrentUserQueryHandler(_userManager, _httpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ReturnsMappedCurrentUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "user@example.com"
        };

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "TestAuthType"))
        };

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { "Admin", "User" });

        // Act
        var result = await _handler.HandleAsync(new GetCurrentUserQuery(), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.CurrentUser.Id);
        Assert.Equal(user.FirstName, result.Value.CurrentUser.FirstName);
        Assert.Equal(user.LastName, result.Value.CurrentUser.LastName);
        Assert.Equal(user.Email, result.Value.CurrentUser.Email);
        Assert.Equal(new[] { "Admin", "User" }, result.Value.CurrentUser.Roles);
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

        _httpContextAccessor.HttpContext.Returns(httpContext);
        _userManager.GetUserAsync(httpContext.User).Returns((ApplicationUser?)null);

        // Act
        var result = await _handler.HandleAsync(new GetCurrentUserQuery(), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotFound, result.Error?.Code);
    }
}
