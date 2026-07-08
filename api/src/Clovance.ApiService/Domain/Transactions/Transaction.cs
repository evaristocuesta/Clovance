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
      Guid createdBy)
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
      Guid createdBy)
    {
        return new Transaction(amount, description, accountId, date, createdBy);
    }

    public static Transaction Create(
        decimal amount,
        string description, 
        Guid accountId,
        DateOnly date,
        Guid createdBy)
    {
        return Create(
            TransactionAmount.Create(amount),
            TransactionDescription.Create(description),
            AccountId.Create(accountId),
            TransactionDate.Create(date),
            createdBy);
    }

    public void ChangeAmount(TransactionAmount amount, Guid modifiedBy)
    {
        if (amount == Amount)
        {
            return;
        }

        Amount = amount;
        MarkAsModified(modifiedBy);
    }

    public void ChangeDate(TransactionDate date, Guid modifiedBy)
    {
        if (date == Date)
        {
            return;
        }

        Date = date;
        MarkAsModified(modifiedBy);
    }

    public void ChangeDescription(TransactionDescription description, Guid modifiedBy)
    {
        if (description == Description)
        {
            return;
        }

        Description = description;
        MarkAsModified(modifiedBy);
    }

    public void MoveToAccount(AccountId accountId, Guid modifiedBy)
    {
        if (accountId == default)
        {
            throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
        }

        if (accountId == AccountId)
        {
            return;
        }

        AccountId = accountId;
        MarkAsModified(modifiedBy);
    }
}
