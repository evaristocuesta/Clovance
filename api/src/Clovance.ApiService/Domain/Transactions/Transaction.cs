using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Transactions;

public sealed class Transaction : AuditableEntityBase<TransactionId>
{
    private Transaction()
    {
    }

    private Transaction(
      TransactionAmount amount,
      TransactionDescription description,
      AccountId accountId,
      TransactionDate transactionDate,
      string createdBy)
    {
        if (accountId == default)
        {
            throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
        }

        Id = TransactionId.New();
        Amount = amount;
        Description = description;
        AccountId = accountId;
        Date = transactionDate;
        MarkAsCreated(createdBy);
    }

    public TransactionAmount Amount { get; private set; } = null!;

    public AccountId AccountId { get; private set; }

    public TransactionDescription Description { get; private set; } = null!;

    public TransactionDate Date { get; private set; } = null!;

    public static Transaction Create(
      TransactionAmount amount,
      TransactionDescription description,
      AccountId accountId,
      TransactionDate date,
      string createdBy)
    {
        return new Transaction(amount, description, accountId, date, createdBy);
    }

    public void ChangeAmount(TransactionAmount amount, string modifiedBy)
    {
        Amount = amount;
        MarkAsModified(modifiedBy);
    }

    public void ChangeDate(TransactionDate date, string modifiedBy)
    {
        Date = date;
        MarkAsModified(modifiedBy);
    }

    public void ChangeDescription(TransactionDescription description, string modifiedBy)
    {
        Description = description;
        MarkAsModified(modifiedBy);
    }

    public void MoveToAccount(AccountId accountId, string modifiedBy)
    {
        if (accountId == default)
        {
            throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
        }

        AccountId = accountId;
        MarkAsModified(modifiedBy);
    }
}
