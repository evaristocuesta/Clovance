using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Accounts;

public sealed class Account : SoftDeletableAuditableEntityBase<AccountId>
{
    private Account()
    {
    }

    private Account(AccountName name, Currency currency, Guid createdBy)
    {
        Id = AccountId.New();
        Name = name;
        Currency = currency;
        MarkAsCreated(createdBy);
    }

    public AccountName Name { get; private set; } = null!;

    public Currency Currency { get; private set; } = null!;

    public static Account Create(AccountName name, Currency currency, Guid createdBy)
    {
        return new Account(name, currency, createdBy);
    }

    public static Account Create(string name, string currency, Guid createdBy)
    {
        return new Account(AccountName.Create(name), Currency.Create(currency), createdBy);
    }

    public void Rename(AccountName name, Guid modifiedBy)
    {
        if (Name.Equals(name))
        {
            return;
        }

        Name = name;
        MarkAsModified(modifiedBy);
    }

    public void Rename(string name, Guid modifiedBy)
    {
        Rename(AccountName.Create(name), modifiedBy);
    }

    public void ChangeCurrency(Currency currency, Guid modifiedBy)
    {
        if (Currency.Equals(currency))
        {
            return;
        }

        Currency = currency;
        MarkAsModified(modifiedBy);
    }

    public void ChangeCurrency(string currency, Guid modifiedBy)
    {
        ChangeCurrency(Currency.Create(currency), modifiedBy);
    }
}
