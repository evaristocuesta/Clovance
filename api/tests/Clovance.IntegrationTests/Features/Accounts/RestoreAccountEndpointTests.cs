using System.Net.Http.Json;
using Clovance.ApiService.Features.Accounts;

namespace Clovance.IntegrationTests.Features.Accounts;

public class RestoreAccountEndpointTests : AccountIntegrationTestBase
{
    public RestoreAccountEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task RestoreAccountEndpoint_ReturnsOk_AndRestoresSoftDeletedAccount()
    {
        AuthenticateAsRegularUser();
        var created = await CreateAccountAsync();
        await Client.DeleteAsync($"/api/accounts/{created.Id}", TestContext.Current.CancellationToken);

        var restoreResponse = await Client.PutAsync($"/api/accounts/{created.Id}/restore", null, TestContext.Current.CancellationToken);
        var getResponse = await Client.GetAsync($"/api/accounts/{created.Id}", TestContext.Current.CancellationToken);
        var account = await ReadJsonAsync<AccountDto>(getResponse.Content);

        Assert.Equal(System.Net.HttpStatusCode.NoContent, restoreResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(account);
        Assert.False(account!.IsDeleted);
    }

    [Fact]
    public async Task RestoreAccountEndpoint_ReturnsNotFound_ForNonExistentAccount()
    {
        AuthenticateAsRegularUser();

        var response = await Client.PutAsync($"/api/accounts/{Guid.CreateVersion7()}/restore", null, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreAccountEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PutAsync($"/api/accounts/{Guid.CreateVersion7()}/restore", null, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
