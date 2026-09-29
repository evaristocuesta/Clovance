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
            AccountType.Checking,
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
            AccountType.Checking,
            Currency.Create("EUR"),
            userId);

        account.Rename(AccountName.Create("Savings"), userId);

        Assert.Equal("Savings", account.Name.Value);
        Assert.Equal(userId, account.ModifiedBy);
        Assert.NotNull(account.ModifiedAt);
    }

    [Fact]
    public void Create_WithStringArguments_NormalizesValues()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            "  Savings  ",
            AccountType.Savings,
            "eur",
            userId);

        Assert.Equal("Savings", account.Name.Value);
        Assert.Equal(AccountType.Savings, account.Type);
        Assert.Equal("EUR", account.Currency.Code);
        Assert.Equal(userId, account.CreatedBy);
    }

    [Fact]
    public void Rename_WhenSameValue_DoesNotUpdateAuditFields()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            AccountType.Checking,
            Currency.Create("EUR"),
            userId);

        account.Rename("Checking", Guid.CreateVersion7());

        Assert.Equal("Checking", account.Name.Value);
        Assert.Null(account.ModifiedBy);
        Assert.Null(account.ModifiedAt);
    }

    [Fact]
    public void ChangeType_UpdatesTypeAndClassificationFlags()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            AccountType.Checking,
            Currency.Create("EUR"),
            userId);

        account.ChangeType(AccountType.Loan, userId);

        Assert.Equal(AccountType.Loan, account.Type);
        Assert.True(account.IsLiability);
        Assert.False(account.IsAsset);
        Assert.True(account.CanBeUsedForToLoanPayment);
        Assert.False(account.CanBeUsedForTransfer);
        Assert.Equal(userId, account.ModifiedBy);
    }

    [Fact]
    public void ChangeCurrency_UpdatesCurrencyAndModifiedAudit()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            AccountType.Checking,
            Currency.Create("EUR"),
            userId);

        account.ChangeCurrency("usd", userId);

        Assert.Equal("USD", account.Currency.Code);
        Assert.Equal(userId, account.ModifiedBy);
        Assert.NotNull(account.ModifiedAt);
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var action = () => Account.Create(
            "",
            AccountType.Checking,
            "EUR",
            Guid.CreateVersion7());

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_WithInvalidCurrency_ThrowsArgumentException()
    {
        var action = () => Account.Create(
            "Checking",
            AccountType.Checking,
            "ZZZ",
            Guid.CreateVersion7());

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void SoftDelete_AndRestore_AreIdempotent()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            AccountType.Checking,
            Currency.Create("EUR"),
            userId);

        account.SoftDelete(userId);
        account.SoftDelete(Guid.CreateVersion7());

        Assert.True(account.IsDeleted);
        Assert.Equal(userId, account.DeletedBy);
        Assert.NotNull(account.DeletedAt);

        account.Restore(userId);
        account.Restore(Guid.CreateVersion7());

        Assert.False(account.IsDeleted);
        Assert.Null(account.DeletedBy);
        Assert.Null(account.DeletedAt);
        Assert.Equal(userId, account.ModifiedBy);
    }

    [Fact]
    public void SoftDelete_ThenRestore_ChangesDeleteState()
    {
        var userId = Guid.CreateVersion7();

        var account = Account.Create(
            AccountName.Create("Checking"),
            AccountType.Checking,
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
