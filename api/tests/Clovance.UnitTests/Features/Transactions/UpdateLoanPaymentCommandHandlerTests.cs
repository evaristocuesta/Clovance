using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Transactions.UpdateLoanPayment;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Clovance.UnitTests.Features.Transactions;

public class UpdateLoanPaymentCommandHandlerTests : TransactionHandlerTestBase
{
    private readonly UpdateLoanPaymentCommandHandler _handler;

    public UpdateLoanPaymentCommandHandlerTests()
    {
        _handler = new UpdateLoanPaymentCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenLoanPaymentExistsAndUserIsAuthenticated_UpdatesLoanPaymentPair()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);

        var payment = CreateLoanPaymentPair(fromAccount.Id.Value, toAccount.Id.Value, 400m, 150m, "Mortgage payment", new DateOnly(2025, 7, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { payment.From, payment.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var command = new UpdateLoanPaymentCommand(
            payment.From.Id.Value,
            new DateOnly(2025, 7, 10),
            "Updated mortgage payment",
            600m,
            250m,
            fromAccount.Id.Value,
            toAccount.Id.Value);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(-600m, result.Value.FromTransaction.Amount);
        Assert.Equal(-250m, result.Value.FromTransaction.PrincipalAmount);
        Assert.Equal(250m, result.Value.ToTransaction.Amount);
        Assert.Equal("Updated mortgage payment", result.Value.FromTransaction.Description);

        var updated = await Context.Transactions.FindAsync([payment.From.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(updated);
        Assert.Equal(-600m, updated.Amount.Value);
        Assert.Equal(-250m, updated.PrincipalAmount.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionNotFound_ReturnsTransactionNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateLoanPaymentCommand(Guid.NewGuid(), new DateOnly(2025, 7, 10), "Updated payment", 600m, 250m, Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Transactions.TransactionNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenFromAccountIsInvalid_ReturnsInvalidAccountError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Loan", AccountType.Loan, "EUR", userId);
        var toAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);

        var payment = CreateLoanPaymentPair(fromAccount.Id.Value, toAccount.Id.Value, 400m, 150m, "Mortgage payment", new DateOnly(2025, 7, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { payment.From, payment.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateLoanPaymentCommand(payment.From.Id.Value, new DateOnly(2025, 7, 10), "Updated payment", 600m, 250m, fromAccount.Id.Value, toAccount.Id.Value),
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

        var payment = CreateLoanPaymentPair(fromAccount.Id.Value, toAccount.Id.Value, 400m, 150m, "Mortgage payment", new DateOnly(2025, 7, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { payment.From, payment.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateLoanPaymentCommand(payment.From.Id.Value, new DateOnly(2025, 7, 10), "Updated payment", 600m, 250m, fromAccount.Id.Value, toAccount.Id.Value),
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

        var payment = CreateLoanPaymentPair(Guid.NewGuid(), toAccount.Id.Value, 400m, 150m, "Mortgage payment", new DateOnly(2025, 7, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { payment.From, payment.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateLoanPaymentCommand(payment.From.Id.Value, new DateOnly(2025, 7, 10), "Updated payment", 600m, 250m, Guid.NewGuid(), toAccount.Id.Value),
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

        var payment = CreateLoanPaymentPair(fromAccount.Id.Value, Guid.NewGuid(), 400m, 150m, "Mortgage payment", new DateOnly(2025, 7, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { payment.From, payment.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateLoanPaymentCommand(payment.From.Id.Value, new DateOnly(2025, 7, 10), "Updated payment", 600m, 250m, fromAccount.Id.Value, Guid.NewGuid()),
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

        var payment = CreateLoanPaymentPair(fromAccount.Id.Value, toAccount.Id.Value, 400m, 150m, "Mortgage payment", new DateOnly(2025, 7, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { payment.From, payment.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await _handler.HandleAsync(
            new UpdateLoanPaymentCommand(payment.From.Id.Value, new DateOnly(2025, 7, 10), "Updated payment", 600m, 250m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }
}
