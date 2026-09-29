using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;

namespace Clovance.UnitTests.Domain.Transactions;

public class TransactionTests
{
    [Fact]
    public void Create_SetsRequiredDataAndAudit()
    {
        var userId = Guid.CreateVersion7();
        var accountId = AccountId.New();

        var transaction = Transaction.Create(
            TransactionAmount.Create(-20.50m),
            TransactionType.Expense,
            TransactionDescription.Create("Dinner"),
            accountId,
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            userId);

        Assert.NotEqual(default, transaction.Id);
        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal(userId, transaction.CreatedBy);
        Assert.True(transaction.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_WithDefaultAccountId_ThrowsArgumentException()
    {
        var userId = Guid.CreateVersion7();

        var action = () => Transaction.Create(
            TransactionAmount.Create(10m),
            TransactionType.Income,
            TransactionDescription.Create("Salary"),
            default,
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            userId);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_WithInvalidAmountForType_ThrowsInvalidOperationException()
    {
        var userId = Guid.CreateVersion7();

        var action = () => Transaction.Create(
            TransactionAmount.Create(20m),
            TransactionType.Expense,
            TransactionDescription.Create("Groceries"),
            AccountId.New(),
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            userId);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void CreateTransfer_CreatesMatchingFromAndToTransactions()
    {
        var userId = Guid.CreateVersion7();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var transfer = Transaction.CreateTransfer(
            150m,
            "House rent",
            fromAccountId,
            toAccountId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            userId);

        Assert.Equal(TransactionType.Transfer, transfer.From.Type);
        Assert.Equal(TransactionType.Transfer, transfer.To.Type);
        Assert.Equal(-150m, transfer.From.Amount.Value);
        Assert.Equal(150m, transfer.To.Amount.Value);
        Assert.Equal("House rent", transfer.From.Description.Value);
        Assert.Equal("House rent", transfer.To.Description.Value);
    }

    [Fact]
    public void CreateLoanPayment_WithValidPrincipal_CreatesPairOfTransactions()
    {
        var userId = Guid.CreateVersion7();

        var loanPayment = Transaction.CreateLoanPayment(
            100m,
            30m,
            "Mortgage payment",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            userId);

        Assert.Equal(TransactionType.LoanPayment, loanPayment.From.Type);
        Assert.Equal(TransactionType.LoanPayment, loanPayment.To.Type);
        Assert.Equal(-100m, loanPayment.From.Amount.Value);
        Assert.Equal(30m, loanPayment.To.Amount.Value);
        Assert.Equal(-30m, loanPayment.From.PrincipalAmount.Value);
        Assert.Null(loanPayment.To.PrincipalAmount.Value);
    }

    [Fact]
    public void CreateLoanPayment_WithPrincipalOutOfRange_ThrowsArgumentException()
    {
        var userId = Guid.CreateVersion7();

        var action = () => Transaction.CreateLoanPayment(
            100m,
            150m,
            "Loan payment",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            userId);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ChangePrincipalAmount_WhenTransactionIsLoanPayment_UpdatesPrincipalAndAudit()
    {
        var userId = Guid.CreateVersion7();
        var loanPayment = Transaction.CreateLoanPayment(
            100m,
            30m,
            "Mortgage payment",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            userId);

        loanPayment.From.ChangePrincipalAmount(TransactionPrincipalAmount.Create(-25m), userId);

        Assert.Equal(-25m, loanPayment.From.PrincipalAmount.Value);
        Assert.Equal(userId, loanPayment.From.ModifiedBy);
        Assert.NotNull(loanPayment.From.ModifiedAt);
    }

    [Fact]
    public void ChangePrincipalAmount_WhenTransactionIsNotLoanPayment_ThrowsInvalidOperationException()
    {
        var userId = Guid.CreateVersion7();
        var transaction = Transaction.Create(
            TransactionAmount.Create(10m),
            TransactionType.Income,
            TransactionDescription.Create("Salary"),
            AccountId.New(),
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            userId);

        var action = () => transaction.ChangePrincipalAmount(TransactionPrincipalAmount.Create(5m), userId);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void MoveToAccount_WithEmptyAccountId_ThrowsArgumentException()
    {
        var userId = Guid.CreateVersion7();
        var transaction = Transaction.Create(
            TransactionAmount.Create(10m),
            TransactionType.Income,
            TransactionDescription.Create("Salary"),
            AccountId.New(),
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            userId);

        var action = () => transaction.MoveToAccount(default, userId);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ChangeDescription_UpdatesDescriptionAndAudit()
    {
        var userId = Guid.CreateVersion7();

        var transaction = Transaction.Create(
            TransactionAmount.Create(10m),
            TransactionType.Income,
            TransactionDescription.Create("Old"),
            AccountId.New(),
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            userId);

        transaction.ChangeDescription(TransactionDescription.Create("New"), userId);

        Assert.Equal("New", transaction.Description.Value);
        Assert.Equal(userId, transaction.ModifiedBy);
        Assert.NotNull(transaction.ModifiedAt);
    }
}
