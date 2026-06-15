using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.CreateInvitation;

namespace Clovance.IntegrationTests.Features.Auth;

public class CreateInvitationEndpointTests : IntegrationTestBase
{
    public CreateInvitationEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateInvitationEndpoint_ReturnSuccess()
    {
        // Arrange
        var email = $"test-{Guid.NewGuid()}@example.com";

        // Get an authenticated admin token
        var (adminToken, adminRefreshToken) = await EnsureAdminReadyAsync();
        AuthenticateWithToken(adminToken);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/invitations",
            new CreateInvitationCommand(Email: email, IsAdmin: false),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvitationEndpoint_ReturnsForbidden_ForNotAdminUser()
    {
        // Arrange
        AuthenticateAsRegularUser();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/invitations",
            new CreateInvitationCommand(Email: "test@example.com", IsAdmin: false),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvitationEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        // Arrange - authenticate with admin user
        AuthenticateAsAdminUser();

        // Act - call the create invitation endpoint with invalid data
        var response = await Client.PostAsJsonAsync("/api/auth/invitations",
            new CreateInvitationCommand("", false), // Invalid: empty email
            TestContext.Current.CancellationToken);

        // Assert - should return 400 Bad Request
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvitationEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        // Arrange - do not authenticate
        AuthenticateWithToken(string.Empty);

        // Act - call the create invitation endpoint
        var response = await Client.PostAsJsonAsync("/api/auth/invitations",
            new CreateInvitationCommand("test@example.com", false),
            TestContext.Current.CancellationToken);

        // Assert - should return 401 Unauthorized
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
