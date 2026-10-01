using Clovance.ApiService.Features.Summary.GetMonthlyBalance;

namespace Clovance.IntegrationTests.Features.Summary;

public class GetMonthlyBalanceEndpointTests : SummaryIntegrationTestBase
{
    public GetMonthlyBalanceEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetMonthlyBalanceEndpoint_ReturnsMonthlyBalance()
    {
        AuthenticateAsRegularUser();

        var account = await CreateAccountAsync(
            name: $"Monthly-Balance-{Guid.CreateVersion7()}",
            type: Clovance.ApiService.Domain.Accounts.AccountType.Checking,
            currency: "EUR",
            openingBalance: 1000m,
            openingDate: new DateOnly(2025, 1, 1),
            openingDescription: "Opening balance");

        await CreateTransactionAsync(account.Id, amount: 300m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Income, description: "Salary", date: new DateOnly(2025, 1, 10));
        await CreateTransactionAsync(account.Id, amount: -120m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Expense, description: "Rent", date: new DateOnly(2025, 1, 25));

        var response = await Client.GetAsync($"/api/summary/monthly-balance?currency=EUR&month=1&year=2025&accountType=Asset", TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<GetMonthlyBalanceResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result!.Points, x => x.Year == 2025 && x.Month == 1 && x.Balance == 1180m);
    }

    [Fact]
    public async Task GetMonthlyBalanceEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync("/api/summary/monthly-balance?currency=EUR&month=1&year=2025", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
