using Clovance.ApiService.Features.Summary.GetMonthlyCashflow;

namespace Clovance.IntegrationTests.Features.Summary;

public class GetMonthlyCashflowEndpointTests : SummaryIntegrationTestBase
{
    public GetMonthlyCashflowEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetMonthlyCashflowEndpoint_ReturnsCashflowForMonth()
    {
        AuthenticateAsRegularUser();

        var account = await CreateAccountAsync(
            name: $"Monthly-Cashflow-{Guid.CreateVersion7()}",
            type: Clovance.ApiService.Domain.Accounts.AccountType.Checking,
            currency: "EUR",
            openingBalance: 0m,
            openingDate: new DateOnly(2025, 1, 1),
            openingDescription: "Opening balance");

        await CreateTransactionAsync(account.Id, amount: 300m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Income, description: "Salary", date: new DateOnly(2025, 1, 10));
        await CreateTransactionAsync(account.Id, amount: -80m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Expense, description: "Rent", date: new DateOnly(2025, 1, 25));

        var response = await Client.GetAsync($"/api/summary/monthly-cashflow?currency=EUR&month=1&year=2025&accountType=Asset", TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<GetMonthlyCashflowResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result!.Points, x => x.Year == 2025 && x.Month == 1 && x.Income == 300m && x.Expenses == -80m);
    }

    [Fact]
    public async Task GetMonthlyCashflowEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync("/api/summary/monthly-cashflow?currency=EUR&month=1&year=2025", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
