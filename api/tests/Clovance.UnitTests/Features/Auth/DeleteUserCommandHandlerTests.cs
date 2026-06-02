using Clovance.ApiService.Features.Auth.DeleteUser;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class DeleteUserCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _handler = new DeleteUserCommandHandler(_userManager);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteUser()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "testuser" };
        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
        _userManager.DeleteAsync(user).Returns(IdentityResult.Success);

        var command = new DeleteUserCommand(UserId: "testuser");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _userManager.Received(1).DeleteAsync(user);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        var command = new DeleteUserCommand(UserId: "nonexistentuser");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotFound, result.Error?.Code);
        await _userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenDeleteFails()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "testuser" };
        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
        _userManager.DeleteAsync(user).Returns(IdentityResult.Failed());

        var command = new DeleteUserCommand(UserId: "testuser");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserDeletionFailed, result.Error?.Code);
        await _userManager.Received(1).DeleteAsync(user);
    }
}
