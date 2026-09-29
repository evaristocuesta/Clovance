using Clovance.ApiService.Features.Accounts.GetCurrencies;

namespace Clovance.UnitTests.Features.Accounts;

public class GetCurrenciesQueryHandlerTests
{
    private readonly GetCurrenciesQueryHandler _handler = new();

    [Fact]
    public async Task HandleAsync_ReturnsCurrencyList()
    {
        var result = await _handler.HandleAsync(new GetCurrenciesQuery(), TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Currencies);
        Assert.Contains(result.Currencies, c => c.Code == "USD");
        Assert.Contains(result.Currencies, c => c.Code == "EUR");
    }
}
