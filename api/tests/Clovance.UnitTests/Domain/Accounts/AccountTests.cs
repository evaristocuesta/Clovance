using Clovance.ApiService.Domain.Accounts;

namespace Clovance.UnitTests.Domain.Accounts;

public class AccountTests
{
    [Fact]
    public void Create_SetsAuditFieldsAndId()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            Currency.Create("EUR"),
            userId);

        Assert.NotEqual(default, account.Id);
        Assert.Equal(userId, account.CreatedBy);
        Assert.True(account.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Rename_UpdatesNameAndModifiedAudit()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            Currency.Create("EUR"),
            userId);

        account.Rename(AccountName.Create("Savings"), userId);

        Assert.Equal("Savings", account.Name.Value);
        Assert.Equal(userId, account.ModifiedBy);
        Assert.NotNull(account.ModifiedAt);
    }

    [Fact]
    public void SoftDelete_ThenRestore_ChangesDeleteState()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            Currency.Create("EUR"),
            userId);

        account.SoftDelete(userId);

        Assert.True(account.IsDeleted);
        Assert.Equal(userId, account.DeletedBy);
        Assert.NotNull(account.DeletedAt);

        account.Restore(userId);

        Assert.False(account.IsDeleted);
        Assert.Null(account.DeletedBy);
        Assert.Null(account.DeletedAt);
        Assert.Equal(userId, account.ModifiedBy);
    }
}
