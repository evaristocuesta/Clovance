using Clovance.ApiService.Features.Auth.RegisterAdmin;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class SetupCommandHandlerTests : AuthHandlerTestBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SetupCommandHandler _handler;

    public SetupCommandHandlerTests()
    {
        _userManager = CreateUserManager();
        _handler = new SetupCommandHandler(_userManager);
    }

    [Fact]
    public async Task HandleAsync_WhenNoUsersExist_CreatesAdminUserAndAddsAdminRole()
    {
        // Arrange
        var command = new SetupCommand(
            "admin@example.com",
            "Password123!",
            "Admin",
            "User");

        _userManager.Users.Returns(new List<ApplicationUser>().AsQueryable());
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "Admin")
            .Returns(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        await _userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u =>
                u.UserName == command.Email &&
                u.Email == command.Email &&
                u.EmailConfirmed &&
                u.FirstName == command.FirstName &&
                u.LastName == command.LastName),
            command.Password);

        await _userManager.Received(1).AddToRoleAsync(
            Arg.Any<ApplicationUser>(),
            "Admin");
    }

    [Fact]
    public async Task HandleAsync_WhenUsersAlreadyExist_ReturnsSetupAlreadyBeenCompletedError()
    {
        // Arrange
        var command = new SetupCommand(
            "admin@example.com",
            "Password123!",
            "Admin",
            "User");

        _userManager.Users.Returns(new List<ApplicationUser>
        {
            new() { Email = "existing@example.com" }
        }.AsQueryable());

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.SetupAlreadyBeenCompleted, result.Error?.Code);
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenCreateUserFails_ReturnsUserCreationFailedError()
    {
        // Arrange
        var command = new SetupCommand(
            "admin@example.com",
            "Password123!",
            "Admin",
            "User");

        _userManager.Users.Returns(new List<ApplicationUser>().AsQueryable());
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserCreationFailed, result.Error?.Code);
        Assert.Contains("Password too weak", result.Error?.Description);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenAddingRoleFails_ReturnsUserCreationFailedError()
    {
        // Arrange
        var command = new SetupCommand(
            "admin@example.com",
            "Password123!",
            "Admin",
            "User");

        _userManager.Users.Returns(new List<ApplicationUser>().AsQueryable());
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "Admin")
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserCreationFailed, result.Error?.Code);
        Assert.Contains("Role assignment failed", result.Error?.Description);
    }
}
