using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Transactions.CreateTranfer;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Transactions;

public class CreateTransferCommandHandlerTests : TransactionHandlerTestBase
{
    private readonly CreateTransferCommandHandler _handler;

    public CreateTransferCommandHandlerTests()
    {
        _handler = new CreateTransferCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAuthenticatedAndAccountsAreValid_CreatesTransferPair()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var command = new CreateTransferCommand(
            new DateOnly(2025, 2, 1),
            "Move funds",
            200m,
            fromAccount.Id.Value,
            toAccount.Id.Value);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(-200m, result.Value.FromTransaction.Amount);
        Assert.Equal(200m, result.Value.ToTransaction.Amount);
        Assert.Equal(result.Value.ToTransaction.Id, result.Value.FromTransaction.RelatedTransactionId);
        Assert.Equal(result.Value.FromTransaction.Id, result.Value.ToTransaction.RelatedTransactionId);

        var transactions = await Context.Transactions.OrderBy(t => t.Amount.Value).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, transactions.Count);
        Assert.All(transactions, t => Assert.Equal("Move funds", t.Description.Value));
    }

    [Fact]
    public async Task HandleAsync_WhenFromAccountIsInvalid_ReturnsInvalidAccountError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);
        var toAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new CreateTransferCommand(new DateOnly(2025, 2, 1), "Move funds", 200m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountTypeInvalid, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await _handler.HandleAsync(
            new CreateTransferCommand(new DateOnly(2025, 2, 1), "Move funds", 200m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }
}
