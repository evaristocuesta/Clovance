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
