using System.Net.Http.Json;
using Clovance.ApiService.Domain.Accounts;

namespace Clovance.IntegrationTests.Features.Accounts;

public class GetCurrenciesEndpointTests : AccountIntegrationTestBase
{
    public GetCurrenciesEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetCurrenciesEndpoint_ReturnsCurrencies()
    {
        AuthenticateAsRegularUser();

        var response = await Client.GetAsync("/api/accounts/currencies", TestContext.Current.CancellationToken);
        var currencies = await ReadJsonAsync<List<CurrencyInfo>>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(currencies);
        Assert.NotEmpty(currencies!);
        Assert.Contains(currencies!, currency => currency.Code == "EUR");
    }

    [Fact]
    public async Task GetCurrenciesEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync("/api/accounts/currencies", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
