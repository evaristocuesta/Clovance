using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions.DeleteTransaction;
using Clovance.ApiService.Shared;
using Microsoft.EntityFrameworkCore;

namespace Clovance.UnitTests.Features.Transactions;

public class DeleteTransactionCommandHandlerTests : TransactionHandlerTestBase
{
    private readonly DeleteTransactionCommandHandler _handler;

    public DeleteTransactionCommandHandlerTests()
    {
        _handler = new DeleteTransactionCommandHandler(Context);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionExists_DeletesTransaction()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var transaction = Transaction.Create(120m, TransactionType.Income, "Salary", account.Id.Value, new DateOnly(2025, 8, 1), userId);

        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);
        await Context.Transactions.AddAsync(transaction, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(new DeleteTransactionCommand(transaction.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Null(await Context.Transactions.FindAsync([transaction.Id], TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenTransferHasRelatedTransaction_DeletesBothTransactions()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);

        var transfer = CreateTransferPair(fromAccount.Id.Value, toAccount.Id.Value, 150m, "Move funds", new DateOnly(2025, 8, 2), userId);
        await Context.Transactions.AddRangeAsync(new[] { transfer.From, transfer.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(new DeleteTransactionCommand(transfer.From.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Empty(await Context.Transactions.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionNotFound_ReturnsTransactionNotFoundError()
    {
        var result = await _handler.HandleAsync(new DeleteTransactionCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Transactions.TransactionNotFound, result.Error?.Code);
    }
}
