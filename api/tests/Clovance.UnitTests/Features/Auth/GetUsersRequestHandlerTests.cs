using System;
using System.Collections.Generic;
using System.Text;
using Clovance.ApiService.Features.Auth.GetUsers;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class GetUsersRequestHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GetUsersRequestHandler _handler;

    public GetUsersRequestHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);
        
        _handler = new GetUsersRequestHandler(_userManager);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUsers_WhenUsersExist()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", UserName = "user1@example.com", Email = "user1@example.com" },
            new ApplicationUser { Id = "user2", UserName = "user2@example.com", Email = "user2@example.com" }
        };

        _userManager.Users.Returns(users.AsQueryable());

        var request = new GetUsersRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Users.Count());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _userManager.Users.Returns(Enumerable.Empty<ApplicationUser>().AsQueryable());
        var request = new GetUsersRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Users);
    }
}
