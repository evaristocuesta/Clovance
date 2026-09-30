using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions.GetTransactions;

namespace Clovance.UnitTests.Features.Transactions;

public class GetTransactionsQueryHandlerTests : TransactionHandlerTestBase
{
    private readonly GetTransactionsQueryHandler _handler;

    public GetTransactionsQueryHandlerTests()
    {
        _handler = new GetTransactionsQueryHandler(Context);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionsMatchFilters_ReturnsFilteredPage()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var savings = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { checking, savings }, TestContext.Current.CancellationToken);

        var salary = Transaction.Create(1500m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 10), userId);
        var groceries = Transaction.Create(-75m, TransactionType.Expense, "Groceries", checking.Id.Value, new DateOnly(2025, 1, 12), userId);
        var bonus = Transaction.Create(300m, TransactionType.Income, "Bonus", savings.Id.Value, new DateOnly(2025, 1, 14), userId);

        await Context.Transactions.AddRangeAsync(new[] { salary, groceries, bonus }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetTransactionsQuery(
                DateFrom: new DateOnly(2025, 1, 1),
                DateTo: new DateOnly(2025, 1, 31),
                AccountId: checking.Id.Value,
                Description: null,
                CursorDate: null,
                CursorId: null,
                PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Contains(result.Value.Items, item => item.Description == "Groceries" && item.AccountId == checking.Id.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionsMatchDateRange_ReturnsOnlyTransactionsInRange()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        var early = Transaction.Create(200m, TransactionType.Income, "Early income", checking.Id.Value, new DateOnly(2025, 1, 5), userId);
        var inRange = Transaction.Create(-40m, TransactionType.Expense, "Coffee", checking.Id.Value, new DateOnly(2025, 1, 12), userId);
        var late = Transaction.Create(300m, TransactionType.Income, "Late income", checking.Id.Value, new DateOnly(2025, 2, 2), userId);

        await Context.Transactions.AddRangeAsync(new[] { early, inRange, late }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetTransactionsQuery(
                DateFrom: new DateOnly(2025, 1, 10),
                DateTo: new DateOnly(2025, 1, 20),
                AccountId: null,
                Description: null,
                CursorDate: null,
                CursorId: null,
                PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("Coffee", result.Value.Items[0].Description);
        Assert.Equal(checking.Id.Value, result.Value.Items[0].AccountId);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountIdFilterIsProvided_ReturnsOnlyTransactionsForThatAccount()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var savings = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { checking, savings }, TestContext.Current.CancellationToken);

        var checkingTx = Transaction.Create(120m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 15), userId);
        var savingsTx = Transaction.Create(50m, TransactionType.Income, "Interest", savings.Id.Value, new DateOnly(2025, 1, 16), userId);

        await Context.Transactions.AddRangeAsync(new[] { checkingTx, savingsTx }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetTransactionsQuery(
                DateFrom: null,
                DateTo: null,
                AccountId: checking.Id.Value,
                Description: null,
                CursorDate: null,
                CursorId: null,
                PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(checking.Id.Value, result.Value.Items[0].AccountId);
        Assert.Equal("Salary", result.Value.Items[0].Description);
    }

    [Fact]
    public async Task HandleAsync_WhenPageSizeIsExceeded_ReturnsHasMoreAndNextCursor()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        var transactions = new[]
        {
            Transaction.Create(100m, TransactionType.Income, "T1", checking.Id.Value, new DateOnly(2025, 1, 10), userId),
            Transaction.Create(110m, TransactionType.Income, "T2", checking.Id.Value, new DateOnly(2025, 1, 11), userId),
            Transaction.Create(120m, TransactionType.Income, "T3", checking.Id.Value, new DateOnly(2025, 1, 12), userId)
        };

        await Context.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(
            new GetTransactionsQuery(
                DateFrom: null,
                DateTo: null,
                AccountId: checking.Id.Value,
                Description: null,
                CursorDate: null,
                CursorId: null,
                PageSize: 2),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasMore);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.NotNull(result.Value.NextCursorDate);
        Assert.NotNull(result.Value.NextCursorId);
    }

    [Fact]
    public async Task HandleAsync_WhenCursorIsProvided_ReturnsSecondPageWithExactNextSlice()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        var transactions = new[]
        {
            Transaction.Create(100m, TransactionType.Income, "T1", checking.Id.Value, new DateOnly(2025, 1, 10), userId),
            Transaction.Create(110m, TransactionType.Income, "T2", checking.Id.Value, new DateOnly(2025, 1, 11), userId),
            Transaction.Create(120m, TransactionType.Income, "T3", checking.Id.Value, new DateOnly(2025, 1, 12), userId),
            Transaction.Create(130m, TransactionType.Income, "T4", checking.Id.Value, new DateOnly(2025, 1, 13), userId)
        };

        await Context.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var firstPage = await _handler.HandleAsync(
            new GetTransactionsQuery(
                DateFrom: null,
                DateTo: null,
                AccountId: checking.Id.Value,
                Description: null,
                CursorDate: null,
                CursorId: null,
                PageSize: 2),
            TestContext.Current.CancellationToken);

        Assert.True(firstPage.IsSuccess);
        Assert.True(firstPage.Value.HasMore);
        Assert.Equal(2, firstPage.Value.Items.Count);

        var result = await _handler.HandleAsync(
            new GetTransactionsQuery(
                DateFrom: null,
                DateTo: null,
                AccountId: checking.Id.Value,
                Description: null,
                CursorDate: firstPage.Value.NextCursorDate,
                CursorId: firstPage.Value.NextCursorId,
                PageSize: 2),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(new[] { "T2", "T1" }, result.Value.Items.Select(item => item.Description).ToArray());
    }

    [Fact]
    public async Task HandleAsync_WhenNoTransactionsExist_ReturnsEmptyCollection()
    {
        var result = await _handler.HandleAsync(
            new GetTransactionsQuery(null, null, null, null, null, null, 10),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.False(result.Value.HasMore);
    }
}
