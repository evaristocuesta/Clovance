namespace Clovance.IntegrationTests.Features.Auth;

public class DeleteUserEndpointTests : IntegrationTestBase
{
    public DeleteUserEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task DeleteUserEndpoint_ReturnsSuccess()
    {
        // Arrange
        AuthenticateAsAdminUser();
        var user = await CreateTestUserAsync();
        var (token, refreshToken) = await LoginUserAsync(AdminEmail, AdminPassword);
        AuthenticateWithToken(token);

        // Act
        var response = await Client.DeleteAsync($"/api/auth/users/{user.UserID}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUserEndpoint_ReturnsNotFoundForNonExistentUser()
    {
        // Arrange
        AuthenticateAsAdminUser();
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/auth/users/{nonExistentUserId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUserEndpoint_ReturnsBadRequest()
    {
        // Arrange
        AuthenticateAsAdminUser();
        var invalidUserId = "invalid-user-id";

        // Act
        var response = await Client.DeleteAsync($"/api/auth/users/{invalidUserId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUserEndpoint_ReturnsForbiddenForRegularUser()
    {
        // Arrange
        AuthenticateAsRegularUser();
        var user = await CreateTestUserAsync();
        var (token, refreshToken) = await LoginUserAsync(user.Email, user.Password); // Log in as the regular user
        AuthenticateWithToken(token);

        // Act
        var response = await Client.DeleteAsync($"/api/auth/users/{user.UserID}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUserEndpoint_ReturnsUnauthorizedForUnauthenticatedUser()
    {
        // Arrange - do not authenticate
        AuthenticateWithToken(string.Empty); // Clear any existing authentication

        // Act
        var response = await Client.DeleteAsync($"/api/auth/users/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
