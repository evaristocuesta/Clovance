using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Summary.GetDailyBalance;

namespace Clovance.UnitTests.Features.Summary;

public class GetDailyBalanceQueryHandlerTests : SummaryHandlerTestBase
{
    private readonly GetDailyBalanceQueryHandler _handler;

    public GetDailyBalanceQueryHandlerTests()
    {
        _handler = new GetDailyBalanceQueryHandler(Context, CurrencyConverter);
    }

    [Fact]
    public async Task HandleAsync_WhenMonthContainsOpeningBalanceAndDailyTransactions_ReturnsDailyBalanceByDate()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.CreateOpeningBalance(1000m, "Opening balance", checking.Id.Value, new DateOnly(2024, 12, 28), userId),
            Transaction.Create(250m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 2), userId),
            Transaction.Create(-75m, TransactionType.Expense, "Groceries", checking.Id.Value, new DateOnly(2025, 1, 2), userId),
            Transaction.Create(50m, TransactionType.Income, "Refund", checking.Id.Value, new DateOnly(2025, 1, 5), userId),
            Transaction.Create(-25m, TransactionType.Expense, "Coffee", checking.Id.Value, new DateOnly(2025, 1, 5), userId)
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetDailyBalanceQuery(AccountId: null, Year: 2025, Month: 1, Currency: "EUR"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);

        var january1 = result.Value.DailyBalance.Single(x => x.Date == new DateOnly(2025, 1, 1));
        var january2 = result.Value.DailyBalance.Single(x => x.Date == new DateOnly(2025, 1, 2));
        var january5 = result.Value.DailyBalance.Single(x => x.Date == new DateOnly(2025, 1, 5));

        Assert.Equal(1000m, january1.Balance);
        Assert.Equal(1175m, january2.Balance);
        Assert.Equal(1200m, january5.Balance);
    }
}
