using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Features.Auth.RegisterWithInvitation;

namespace Clovance.IntegrationTests.Features.Auth;

public class RegisterWithInvitationEndpointTests : IntegrationTestBase
{
    public RegisterWithInvitationEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task RegisterWithInvitationEndpoint_ReturnsCreated()
    {
        // Arrange
        await EnsureAdminReadyAsync();
        var (token, refreshToken) = await LoginUserAsync(AdminEmail, NewAdminPassword);
        AuthenticateWithToken(token);

        var responseInvitation = await Client.PostAsJsonAsync(
            "/api/auth/invitations",
            new CreateInvitationCommand(Email: "test1@example.com"),
            TestContext.Current.CancellationToken);

        var invitation = await responseInvitation.Content.ReadFromJsonAsync<CreateInvitationResult>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/register-with-invitation",
            new RegisterWithInvitationCommand(Email: "test1@example.com", Password: "Password123!", Token: invitation!.Token),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task RegisterWithInvitationEndpoint_ReturnsBadRequest()
    {
        // Arrange

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/register-with-invitation",
            new RegisterWithInvitationCommand(Email: "", Password: "", Token: ""),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterWithInvitationEndpoint_ReturnsUnauthorizedForInvalidToken()
    {
        // Arrange
        await EnsureAdminReadyAsync();
        var (token, refreshToken) = await LoginUserAsync(AdminEmail, NewAdminPassword);
        AuthenticateWithToken(token);

        var responseInvitation = await Client.PostAsJsonAsync(
            "/api/auth/invitations",
            new CreateInvitationCommand(Email: "test2@example.com"),
            TestContext.Current.CancellationToken);

        var invitation = await responseInvitation.Content.ReadFromJsonAsync<CreateInvitationResult>(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/register-with-invitation",
            new RegisterWithInvitationCommand(Email: "test2@example.com", Password: "Password123!", Token: "invalid-token"),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
