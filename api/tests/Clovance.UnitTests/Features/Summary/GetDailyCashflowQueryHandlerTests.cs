using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Summary.GetDailyCashflow;

namespace Clovance.UnitTests.Features.Summary;

public class GetDailyCashflowQueryHandlerTests : SummaryHandlerTestBase
{
    private readonly GetDailyCashflowQueryHandler _handler;

    public GetDailyCashflowQueryHandlerTests()
    {
        _handler = new GetDailyCashflowQueryHandler(Context, CurrencyConverter);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionsExist_ReturnsIncomeAndExpensesPerDay()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.Create(500m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 2), userId),
            Transaction.Create(-150m, TransactionType.Expense, "Groceries", checking.Id.Value, new DateOnly(2025, 1, 2), userId),
            Transaction.Create(200m, TransactionType.Income, "Side gig", checking.Id.Value, new DateOnly(2025, 1, 10), userId)
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetDailyCashflowQuery(AccountId: null, Year: 2025, Month: 1, Currency: "EUR"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);

        var january2 = result.Value.DailyClashFlow.Single(x => x.Date == new DateOnly(2025, 1, 2));

        Assert.Equal(500m, january2.Income);
        Assert.Equal(-150m, january2.Expenses);
        Assert.NotNull(january2.ByAccount);
        Assert.Contains(january2.ByAccount!, x => x.AccountId == checking.Id.Value && x.Income == 500m && x.Expenses == -150m);
    }
}
