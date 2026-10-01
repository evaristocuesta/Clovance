using System.Net.Http.Json;
using Clovance.ApiService.Features.Accounts;

namespace Clovance.IntegrationTests.Features.Accounts;

public class DeleteAccountEndpointTests : AccountIntegrationTestBase
{
    public DeleteAccountEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task DeleteAccountEndpoint_ReturnsNoContent_AndSoftDeletesAccount()
    {
        AuthenticateAsRegularUser();
        var created = await CreateAccountAsync();

        var deleteResponse = await Client.DeleteAsync($"/api/accounts/{created.Id}", TestContext.Current.CancellationToken);
        var getResponse = await Client.GetAsync($"/api/accounts/{created.Id}", TestContext.Current.CancellationToken);
        var account = await ReadJsonAsync<AccountDto>(getResponse.Content);

        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(account);
        Assert.True(account!.IsDeleted);
    }

    [Fact]
    public async Task DeleteAccountEndpoint_ReturnsNotFound_ForNonExistentAccount()
    {
        AuthenticateAsRegularUser();

        var response = await Client.DeleteAsync($"/api/accounts/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccountEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.DeleteAsync($"/api/accounts/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
