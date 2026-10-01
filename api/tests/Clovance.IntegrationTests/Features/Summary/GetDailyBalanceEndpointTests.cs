using Clovance.ApiService.Features.Summary.GetDailyBalance;

namespace Clovance.IntegrationTests.Features.Summary;

public class GetDailyBalanceEndpointTests : SummaryIntegrationTestBase
{
    public GetDailyBalanceEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetDailyBalanceEndpoint_ReturnsBalanceForMonth()
    {
        AuthenticateAsRegularUser();

        var account = await CreateAccountAsync(
            name: $"Daily-Balance-{Guid.CreateVersion7()}",
            type: Clovance.ApiService.Domain.Accounts.AccountType.Checking,
            currency: "EUR",
            openingBalance: 1000m,
            openingDate: new DateOnly(2025, 1, 1),
            openingDescription: "Opening balance");

        await CreateTransactionAsync(account.Id, amount: 250m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Income, description: "Salary", date: new DateOnly(2025, 1, 2));
        await CreateTransactionAsync(account.Id, amount: -50m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Expense, description: "Groceries", date: new DateOnly(2025, 1, 2));

        var response = await Client.GetAsync($"/api/summary/daily-balance?currency=EUR&month=1&year=2025&accountType=Asset", TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<GetDailyBalanceResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result!.DailyBalance, x => x.Date == new DateOnly(2025, 1, 1) && x.Balance == 1000m);
        Assert.Contains(result.DailyBalance, x => x.Date == new DateOnly(2025, 1, 2) && x.Balance == 1200m);
    }

    [Fact]
    public async Task GetDailyBalanceEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync("/api/summary/daily-balance?currency=EUR&month=1&year=2025", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
