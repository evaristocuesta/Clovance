using Clovance.ApiService.Features.Auth.GetUserById;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class GetUserByIdRequestHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GetUserByIdRequestHandler _handler;

    public GetUserByIdRequestHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _handler = new GetUserByIdRequestHandler(_userManager);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser { Id = "testuser", UserName = "testuser@example.com", Email = "testuser@example.com" };
        _userManager.Users.Returns(new[] { user }.AsQueryable());
        _userManager.GetRolesAsync(user).Returns(new List<string> { "Admin" });
        var request = new GetUserByIdRequest(UserId: "testuser");

        // Act
        var result = await _handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("testuser@example.com", result.Value!.User.Email);
        Assert.Contains("Admin", result.Value.User.Roles);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        _userManager.Users.Returns(Enumerable.Empty<ApplicationUser>().AsQueryable());
        var request = new GetUserByIdRequest(UserId: "nonexistentuser");

        // Act
        var result = await _handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotFound, result.Error?.Code);
    }
}
