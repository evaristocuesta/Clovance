using Clovance.ApiService.Features.Summary.GetDailyCashflow;

namespace Clovance.IntegrationTests.Features.Summary;

public class GetDailyCashflowEndpointTests : SummaryIntegrationTestBase
{
    public GetDailyCashflowEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetDailyCashflowEndpoint_ReturnsCashflowForMonth()
    {
        AuthenticateAsRegularUser();

        var account = await CreateAccountAsync(
            name: $"Daily-Cashflow-{Guid.CreateVersion7()}",
            type: Clovance.ApiService.Domain.Accounts.AccountType.Checking,
            currency: "EUR",
            openingBalance: 0m,
            openingDate: new DateOnly(2025, 1, 1),
            openingDescription: "Opening balance");

        await CreateTransactionAsync(account.Id, amount: 250m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Income, description: "Salary", date: new DateOnly(2025, 1, 2));
        await CreateTransactionAsync(account.Id, amount: -60m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Expense, description: "Groceries", date: new DateOnly(2025, 1, 2));

        var response = await Client.GetAsync($"/api/summary/daily-cashflow?currency=EUR&month=1&year=2025&accountType=Asset", TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<GetDailyCashflowResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result!.DailyClashFlow, x => x.Date == new DateOnly(2025, 1, 2) && x.Income == 250m && x.Expenses == -60m);
    }

    [Fact]
    public async Task GetDailyCashflowEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync("/api/summary/daily-cashflow?currency=EUR&month=1&year=2025", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
