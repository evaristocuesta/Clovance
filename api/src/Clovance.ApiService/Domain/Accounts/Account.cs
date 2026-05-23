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

  public void Rename(AccountName name, string modifiedBy)
  {
    Name = name;
    MarkAsModified(modifiedBy);
  }

  public void ChangeCurrency(Currency currency, string modifiedBy)
  {
    Currency = currency;
    MarkAsModified(modifiedBy);
  }
}
