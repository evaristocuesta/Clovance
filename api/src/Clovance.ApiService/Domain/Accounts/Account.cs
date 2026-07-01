using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Accounts;

public sealed class Account : SoftDeletableAuditableEntityBase<AccountId>
{
    private Account()
    {
    }

    private Account(AccountName name, Currency currency, string createdBy)
    {
        Id = AccountId.New();
        Name = name;
        Currency = currency;
        MarkAsCreated(createdBy);
    }

    public AccountName Name { get; private set; } = null!;

    public Currency Currency { get; private set; } = null!;

    public static Account Create(AccountName name, Currency currency, string createdBy)
    {
        return new Account(name, currency, createdBy);
    }

    public static Account Create(string name, string currency, string createdBy)
    {
        return new Account(AccountName.Create(name), Currency.Create(currency), createdBy);
    }

    public void Rename(AccountName name, string modifiedBy)
    {
        if (Name.Equals(name))
        {
            return;
        }

        Name = name;
        MarkAsModified(modifiedBy);
    }

    public void Rename(string name, string modifiedBy)
    {
        Rename(AccountName.Create(name), modifiedBy);
    }

    public void ChangeCurrency(Currency currency, string modifiedBy)
    {
        if (Currency.Equals(currency))
        {
            return;
        }

        Currency = currency;
        MarkAsModified(modifiedBy);
    }

    public void ChangeCurrency(string currency, string modifiedBy)
    {
        ChangeCurrency(Currency.Create(currency), modifiedBy);
    }
}
