using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions.GetTransactionById;
using Clovance.ApiService.Shared;

namespace Clovance.UnitTests.Features.Transactions;

public class GetTransactionByIdQueryHandlerTests : TransactionHandlerTestBase
{
    private readonly GetTransactionByIdQueryHandler _handler;

    public GetTransactionByIdQueryHandlerTests()
    {
        _handler = new GetTransactionByIdQueryHandler(Context);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionExists_ReturnsTransaction()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);

        var transaction = Transaction.Create(120m, TransactionType.Income, "Salary", account.Id.Value, new DateOnly(2025, 4, 1), userId);
        await Context.Transactions.AddAsync(transaction, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(new GetTransactionByIdQuery(transaction.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Transaction);
        Assert.Equal(transaction.Id.Value, result.Value.Transaction!.Id);
        Assert.Equal("Checking", result.Value.Transaction.AccountName);
        Assert.Equal("EUR", result.Value.Transaction.Currency);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionDoesNotExist_ReturnsTransactionNotFoundError()
    {
        var result = await _handler.HandleAsync(new GetTransactionByIdQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Transactions.TransactionNotFound, result.Error?.Code);
    }
}
