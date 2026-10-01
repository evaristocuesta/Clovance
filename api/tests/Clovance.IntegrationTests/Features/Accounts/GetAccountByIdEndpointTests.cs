using System.Net.Http.Json;
using Clovance.ApiService.Features.Accounts;

namespace Clovance.IntegrationTests.Features.Accounts;

public class GetAccountByIdEndpointTests : AccountIntegrationTestBase
{
    public GetAccountByIdEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetAccountByIdEndpoint_ReturnsAccount()
    {
        AuthenticateAsRegularUser();
        var created = await CreateAccountAsync();

        var response = await Client.GetAsync($"/api/accounts/{created.Id}", TestContext.Current.CancellationToken);
        var account = await ReadJsonAsync<AccountDto>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(account);
        Assert.Equal(created.Id, account!.Id);
    }

    [Fact]
    public async Task GetAccountByIdEndpoint_ReturnsNotFound_ForNonExistentAccount()
    {
        AuthenticateAsRegularUser();

        var response = await Client.GetAsync($"/api/accounts/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountByIdEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync($"/api/accounts/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
