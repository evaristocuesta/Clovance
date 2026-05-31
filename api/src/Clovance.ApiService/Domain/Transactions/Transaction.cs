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
        TransactionDate = transactionDate;
        MarkAsCreated(createdBy);
    }

    public TransactionAmount Amount { get; private set; } = null!;

    public AccountId AccountId { get; private set; }

    public TransactionDescription Description { get; private set; } = null!;

    public TransactionDate TransactionDate { get; private set; } = null!;

    public static Transaction Create(
      TransactionAmount amount,
      TransactionDescription description,
      AccountId accountId,
      TransactionDate transactionDate,
      string createdBy)
    {
        return new Transaction(amount, description, accountId, transactionDate, createdBy);
    }

    public void ChangeAmount(TransactionAmount amount, string modifiedBy)
    {
        Amount = amount;
        MarkAsModified(modifiedBy);
    }

    public void ChangeDate(TransactionDate transactionDate, string modifiedBy)
    {
        TransactionDate = transactionDate;
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
