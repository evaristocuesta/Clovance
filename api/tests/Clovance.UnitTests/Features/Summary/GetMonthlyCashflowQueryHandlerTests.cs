using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Summary.GetMonthlyCashflow;

namespace Clovance.UnitTests.Features.Summary;

public class GetMonthlyCashflowQueryHandlerTests : SummaryHandlerTestBase
{
    private readonly GetMonthlyCashflowQueryHandler _handler;

    public GetMonthlyCashflowQueryHandlerTests()
    {
        _handler = new GetMonthlyCashflowQueryHandler(Context, CurrencyConverter);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionsExistAcrossMonths_ReturnsMonthlyCashflowTotals()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.Create(500m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 10), userId),
            Transaction.Create(-150m, TransactionType.Expense, "Groceries", checking.Id.Value, new DateOnly(2025, 1, 15), userId),
            Transaction.Create(250m, TransactionType.Income, "Bonus", checking.Id.Value, new DateOnly(2025, 2, 3), userId),
            Transaction.Create(-75m, TransactionType.Expense, "Utilities", checking.Id.Value, new DateOnly(2025, 2, 17), userId)
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetMonthlyCashflowQuery(AccountId: null, Currency: "EUR", MonthsBack: 2, AnchorYear: 2025, AnchorMonth: 2),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);

        var january = result.Value.Points.Single(x => x.Year == 2025 && x.Month == 1);
        var february = result.Value.Points.Single(x => x.Year == 2025 && x.Month == 2);

        Assert.Equal(500m, january.Income);
        Assert.Equal(-150m, january.Expenses);
        Assert.Equal(250m, february.Income);
        Assert.Equal(-75m, february.Expenses);
    }
}
