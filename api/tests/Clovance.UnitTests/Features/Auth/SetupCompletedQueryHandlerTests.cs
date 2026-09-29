using Clovance.ApiService.Features.Auth.SetupCompleted;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class SetupCompletedQueryHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SetupCompletedQueryHandler _handler;

    public SetupCompletedQueryHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _handler = new SetupCompletedQueryHandler(_userManager);
    }

    [Fact]
    public async Task HandleAsync_WhenUsersExist_ReturnsSetupCompletedTrue()
    {
        // Arrange
        _userManager.Users.Returns(new List<ApplicationUser>
        {
            new() { Email = "admin@example.com" }
        }.AsQueryable());

        // Act
        var result = await _handler.HandleAsync(new SetupCompletedQuery(), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsSetupCompleted);
    }

    [Fact]
    public async Task HandleAsync_WhenNoUsersExist_ReturnsSetupCompletedFalse()
    {
        // Arrange
        _userManager.Users.Returns(new List<ApplicationUser>().AsQueryable());

        // Act
        var result = await _handler.HandleAsync(new SetupCompletedQuery(), TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsSetupCompleted);
    }
}
