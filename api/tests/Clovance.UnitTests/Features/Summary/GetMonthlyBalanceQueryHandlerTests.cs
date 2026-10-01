using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Summary.GetMonthlyBalance;

namespace Clovance.UnitTests.Features.Summary;

public class GetMonthlyBalanceQueryHandlerTests : SummaryHandlerTestBase
{
    private readonly GetMonthlyBalanceQueryHandler _handler;

    public GetMonthlyBalanceQueryHandlerTests()
    {
        _handler = new GetMonthlyBalanceQueryHandler(Context, CurrencyConverter);
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleMonthsArePresent_ReturnsMonthlyBalancesAcrossWindow()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.CreateOpeningBalance(1000m, "Opening balance", checking.Id.Value, new DateOnly(2024, 12, 28), userId),
            Transaction.Create(500m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 10), userId),
            Transaction.Create(-200m, TransactionType.Expense, "Rent", checking.Id.Value, new DateOnly(2025, 1, 25), userId),
            Transaction.Create(-100m, TransactionType.Expense, "Utilities", checking.Id.Value, new DateOnly(2025, 2, 5), userId)
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetMonthlyBalanceQuery(AccountId: null, Currency: "EUR", MonthsBack: 2, AnchorYear: 2025, AnchorMonth: 2),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Points.Count);

        var january = result.Value.Points.Single(x => x.Year == 2025 && x.Month == 1);
        var february = result.Value.Points.Single(x => x.Year == 2025 && x.Month == 2);

        Assert.Equal(1300m, january.Balance);
        Assert.Equal(1200m, february.Balance);
    }
}
