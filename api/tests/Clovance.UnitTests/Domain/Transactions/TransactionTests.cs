using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;

namespace Clovance.UnitTests.Domain.Transactions;

public class TransactionTests
{
    [Fact]
    public void Create_SetsRequiredDataAndAudit()
    {
        var accountId = AccountId.New();

        var transaction = Transaction.Create(
            TransactionAmount.Create(-20.50m),
            TransactionDescription.Create("Dinner"),
            accountId,
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            "creator");

        Assert.NotEqual(default, transaction.Id);
        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal("creator", transaction.CreatedBy);
        Assert.True(transaction.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_WithDefaultAccountId_ThrowsArgumentException()
    {
        var action = () => Transaction.Create(
            TransactionAmount.Create(10m),
            TransactionDescription.Create("Salary"),
            default,
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            "creator");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ChangeDescription_UpdatesDescriptionAndAudit()
    {
        var transaction = Transaction.Create(
            TransactionAmount.Create(10m),
            TransactionDescription.Create("Old"),
            AccountId.New(),
            TransactionDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            "creator");

        transaction.ChangeDescription(TransactionDescription.Create("New"), "editor");

        Assert.Equal("New", transaction.Description.Value);
        Assert.Equal("editor", transaction.ModifiedBy);
        Assert.NotNull(transaction.ModifiedAt);
    }
}
