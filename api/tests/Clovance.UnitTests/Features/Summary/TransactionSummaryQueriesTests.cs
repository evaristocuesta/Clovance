using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Summary.Shared;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Summary;

public class TransactionSummaryQueriesTests : SummaryHandlerTestBase
{
    [Fact]
    public async Task GetDailyFlowsAsync_WhenTransactionsContainIncomeExpenseAndTransfer_ReturnsExpectedBreakdown()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var savings = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { checking, savings }, TestContext.Current.CancellationToken);

        var transfer = Transaction.CreateTransfer(
            80m,
            "Transfer to savings",
            checking.Id.Value,
            savings.Id.Value,
            new DateOnly(2025, 1, 2),
            userId);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.Create(120m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 2), userId),
            Transaction.Create(-40m, TransactionType.Expense, "Groceries", checking.Id.Value, new DateOnly(2025, 1, 2), userId),
            transfer.From,
            transfer.To
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await TransactionSummaryQueries.GetDailyFlowsAsync(
            Context.Transactions.AsNoTracking(),
            new DateOnly(2025, 1, 2),
            new DateOnly(2025, 1, 2),
            null,
            "EUR",
            CurrencyConverter,
            TestContext.Current.CancellationToken);

        var checkingFlow = Assert.Single(result, x => x.AccountId == checking.Id);
        Assert.Equal(120m, checkingFlow.Income);
        Assert.Equal(-40m, checkingFlow.Expenses);
        Assert.Equal(0m, checkingFlow.TransferIn);
        Assert.Equal(-80m, checkingFlow.TransferOut);

        var savingsFlow = Assert.Single(result, x => x.AccountId == savings.Id);
        Assert.Equal(0m, savingsFlow.Income);
        Assert.Equal(0m, savingsFlow.Expenses);
        Assert.Equal(80m, savingsFlow.TransferIn);
        Assert.Equal(0m, savingsFlow.TransferOut);
    }

    [Fact]
    public async Task GetOpeningBalancesAsync_WhenFilteringByLiability_ReturnsOnlyLiabilityAccounts()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var creditCard = CreateAccount("Credit Card", AccountType.CreditCard, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { checking, creditCard }, TestContext.Current.CancellationToken);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.CreateOpeningBalance(1000m, "Opening balance", checking.Id.Value, new DateOnly(2025, 1, 1), userId),
            Transaction.CreateOpeningBalance(250m, "Card opening", creditCard.Id.Value, new DateOnly(2025, 1, 1), userId),
            Transaction.Create(50m, TransactionType.Income, "Bonus", checking.Id.Value, new DateOnly(2025, 2, 1), userId)
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await TransactionSummaryQueries.GetOpeningBalancesAsync(
            Context.Transactions.AsNoTracking(),
            new DateOnly(2025, 2, 1),
            AccountTypeFilter.Liability,
            "EUR",
            CurrencyConverter,
            TestContext.Current.CancellationToken);

        var pair = Assert.Single(result);
        Assert.Equal(creditCard.Id, pair.Key);
        Assert.Equal(250m, pair.Value);
    }

    [Fact]
    public async Task GetDailyFlowsAsync_WhenCurrencyDoesNotMatchTarget_UsesExchangeRateConversion()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "USD", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);

        await Context.Transactions.AddAsync(
            Transaction.Create(100m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 10), userId),
            TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        CurrencyConverter
            .GetExchangeRatesAsync("EUR", Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["USD"] = 1.1m
            });

        var result = await TransactionSummaryQueries.GetDailyFlowsAsync(
            Context.Transactions.AsNoTracking(),
            new DateOnly(2025, 1, 10),
            new DateOnly(2025, 1, 10),
            null,
            "EUR",
            CurrencyConverter,
            TestContext.Current.CancellationToken);

        var flow = Assert.Single(result);
        Assert.Equal(90.9090909091m, flow.Income, 10);
    }

    [Fact]
    public async Task GetOpeningBalancesAsync_WhenExchangeRateMissing_ThrowsInvalidOperationException()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "USD", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);
        await Context.Transactions.AddAsync(
            Transaction.CreateOpeningBalance(100m, "Opening balance", checking.Id.Value, new DateOnly(2025, 1, 1), userId),
            TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        CurrencyConverter
            .GetExchangeRatesAsync("EUR", Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase));

        await Assert.ThrowsAsync<InvalidOperationException>(() => TransactionSummaryQueries.GetOpeningBalancesAsync(
            Context.Transactions.AsNoTracking(),
            new DateOnly(2025, 2, 1),
            null,
            "EUR",
            CurrencyConverter,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetDailyFlowsAsync_WhenAccountTypeFilterIsAsset_OnlyIncludesAssetAccounts()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var creditCard = CreateAccount("Credit Card", AccountType.CreditCard, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { checking, creditCard }, TestContext.Current.CancellationToken);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.Create(100m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 10), userId),
            Transaction.Create(300m, TransactionType.Income, "Card refund", creditCard.Id.Value, new DateOnly(2025, 1, 10), userId)
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await TransactionSummaryQueries.GetDailyFlowsAsync(
            Context.Transactions.AsNoTracking(),
            new DateOnly(2025, 1, 10),
            new DateOnly(2025, 1, 10),
            AccountTypeFilter.Asset,
            "EUR",
            CurrencyConverter,
            TestContext.Current.CancellationToken);

        var flow = Assert.Single(result);
        Assert.Equal(checking.Id, flow.AccountId);
        Assert.Equal(100m, flow.Income);
    }

    [Fact]
    public async Task GetDailyFlowsAsync_WhenTargetCurrencyMatchesAccountCurrency_DoesNotQueryExchangeRates()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(checking, TestContext.Current.CancellationToken);
        await Context.Transactions.AddAsync(
            Transaction.Create(100m, TransactionType.Income, "Salary", checking.Id.Value, new DateOnly(2025, 1, 10), userId),
            TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await TransactionSummaryQueries.GetDailyFlowsAsync(
            Context.Transactions.AsNoTracking(),
            new DateOnly(2025, 1, 10),
            new DateOnly(2025, 1, 10),
            null,
            "EUR",
            CurrencyConverter,
            TestContext.Current.CancellationToken);

        Assert.Single(result);
        Assert.Empty(CurrencyConverter.ReceivedCalls());
    }

    [Fact]
    public async Task GetOpeningBalancesAsync_WhenNoFilterIsProvided_IncludesAllAccounts()
    {
        var userId = Guid.CreateVersion7();
        var checking = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var savings = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { checking, savings }, TestContext.Current.CancellationToken);

        await Context.Transactions.AddRangeAsync(new[]
        {
            Transaction.CreateOpeningBalance(100m, "Opening balance", checking.Id.Value, new DateOnly(2025, 1, 1), userId),
            Transaction.CreateOpeningBalance(50m, "Savings opening", savings.Id.Value, new DateOnly(2025, 1, 1), userId),
            Transaction.Create(25m, TransactionType.Income, "Bonus", checking.Id.Value, new DateOnly(2025, 2, 1), userId)
        }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await TransactionSummaryQueries.GetOpeningBalancesAsync(
            Context.Transactions.AsNoTracking(),
            new DateOnly(2025, 2, 1),
            null,
            "EUR",
            CurrencyConverter,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);
        Assert.Equal(100m, result[checking.Id]);
        Assert.Equal(50m, result[savings.Id]);
    }
}
