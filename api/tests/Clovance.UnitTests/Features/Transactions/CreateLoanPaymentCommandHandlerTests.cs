using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions.CreateLoanPayment;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Transactions;

public class CreateLoanPaymentCommandHandlerTests : TransactionHandlerTestBase
{
    private readonly CreateLoanPaymentCommandHandler _handler;

    public CreateLoanPaymentCommandHandlerTests()
    {
        _handler = new CreateLoanPaymentCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAuthenticatedAndAccountsAreValid_CreatesLoanPaymentPair()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var command = new CreateLoanPaymentCommand(
            new DateOnly(2025, 3, 1),
            "Mortgage payment",
            500m,
            200m,
            fromAccount.Id.Value,
            toAccount.Id.Value);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(-500m, result.Value.FromTransaction.Amount);
        Assert.Equal(-200m, result.Value.FromTransaction.PrincipalAmount);
        Assert.Equal(200m, result.Value.ToTransaction.Amount);
        Assert.Null(result.Value.ToTransaction.PrincipalAmount);

        var transactions = await Context.Transactions.OrderBy(t => t.Amount.Value).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, transactions.Count);
        Assert.Contains(transactions, t => t.Type == TransactionType.LoanPayment && t.PrincipalAmount != null && t.PrincipalAmount.Value == -200m);
    }

    [Fact]
    public async Task HandleAsync_WhenFromAccountIsInvalid_ReturnsInvalidAccountError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("AutoLoan", AccountType.Loan, "EUR", userId);
        var toAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new CreateLoanPaymentCommand(new DateOnly(2025, 3, 1), "Mortgage payment", 500m, 200m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountTypeInvalid, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenToAccountIsInvalid_ReturnsInvalidAccountError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new CreateLoanPaymentCommand(new DateOnly(2025, 3, 1), "Mortgage payment", 500m, 200m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountTypeInvalid, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenFromAccountDoesNotExist_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        var toAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);

        await Context.Accounts.AddAsync(toAccount, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new CreateLoanPaymentCommand(new DateOnly(2025, 3, 1), "Mortgage payment", 500m, 200m, Guid.NewGuid(), toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenToAccountDoesNotExist_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(fromAccount, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new CreateLoanPaymentCommand(new DateOnly(2025, 3, 1), "Mortgage payment", 500m, 200m, fromAccount.Id.Value, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await _handler.HandleAsync(
            new CreateLoanPaymentCommand(new DateOnly(2025, 3, 1), "Mortgage payment", 500m, 200m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }
}
