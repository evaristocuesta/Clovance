using Clovance.ApiService.Domain.Accounts;

namespace Clovance.Tests.Domain.Accounts;

public class AccountTests
{
    [Fact]
    public void Create_SetsAuditFieldsAndId()
    {
        var account = Account.Create(
            AccountName.Create("Checking"),
            Currency.Create("EUR"),
            "user-1");

        Assert.NotEqual(default, account.Id);
        Assert.Equal("user-1", account.CreatedBy);
        Assert.True(account.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Rename_UpdatesNameAndModifiedAudit()
    {
        var account = Account.Create(
            AccountName.Create("Checking"),
            Currency.Create("EUR"),
            "creator");

        account.Rename(AccountName.Create("Savings"), "editor");

        Assert.Equal("Savings", account.Name.Value);
        Assert.Equal("editor", account.ModifiedBy);
        Assert.NotNull(account.ModifiedAt);
    }

    [Fact]
    public void SoftDelete_ThenRestore_ChangesDeleteState()
    {
        var account = Account.Create(
            AccountName.Create("Checking"),
            Currency.Create("EUR"),
            "creator");

        account.SoftDelete("deleter");

        Assert.True(account.IsDeleted);
        Assert.Equal("deleter", account.DeletedBy);
        Assert.NotNull(account.DeletedAt);

        account.Restore("restorer");

        Assert.False(account.IsDeleted);
        Assert.Null(account.DeletedBy);
        Assert.Null(account.DeletedAt);
        Assert.Equal("restorer", account.ModifiedBy);
    }
}
