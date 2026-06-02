using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.Login;

namespace Clovance.IntegrationTests.Features.Auth;

public class LoginEndpointTests : IntegrationTestBase
{
    public LoginEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task LoginEndpoint_ReturnsTokenForValidUser()
    {
        // Arrange
        var user = await CreateTestUserAsync("test1@example.com", "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", 
            new LoginCommand(Email: user.Email, Password: "Password123!"), 
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
    }

    [Fact]
    public async Task LoginEndpoint_ReturnsUnauthorizedForInvalidUser()
    {
        // Arrange
        var user = await CreateTestUserAsync("test2@example.com", "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", 
            new LoginCommand(Email: user.Email, Password: "WrongPassword!"), 
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_ReturnsBadRequestForMissingCredentials()
    {
        // Arrange
        var user = await CreateTestUserAsync("test3@example.com", "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", 
            new LoginCommand(Email: user.Email, Password: ""), 
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
