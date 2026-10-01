using System.Net.Http.Json;
using Clovance.ApiService.Features.Accounts;

namespace Clovance.IntegrationTests.Features.Accounts;

public class GetAccountsEndpointTests : AccountIntegrationTestBase
{
    public GetAccountsEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetAccountsEndpoint_ReturnsAccounts()
    {
        AuthenticateAsRegularUser();
        var first = await CreateAccountAsync(name: $"First-{Guid.CreateVersion7()}");
        var second = await CreateAccountAsync(name: $"Second-{Guid.CreateVersion7()}");

        var response = await Client.GetAsync("/api/accounts", TestContext.Current.CancellationToken);
        var accounts = await ReadJsonAsync<List<AccountDto>>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(accounts);
        Assert.Contains(accounts!, account => account.Id == first.Id);
        Assert.Contains(accounts!, account => account.Id == second.Id);
    }

    [Fact]
    public async Task GetAccountsEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync("/api/accounts", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
