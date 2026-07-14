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
      TransactionType type,
      TransactionDescription description,
      AccountId accountId,
      TransactionDate transactionDate,
      Guid createdBy)
    {
        if (accountId == default)
        {
            throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
        }

        if (!TransactionAmountTypeRules.EnsureAmountMatchesType(amount.Value, amount.Value, type))
        {
            throw new InvalidOperationException($"Amount type '{amount.GetType().Name}' is not valid for transaction type '{type.GetType().Name}'.");
        }

        Id = TransactionId.New();
        Amount = amount;
        Type = type;
        Description = description;
        AccountId = accountId;
        Date = transactionDate;
        MarkAsCreated(createdBy);
    }

    public TransactionAmount Amount { get; private set; } = null!;
    
    public TransactionType Type { get; private set; }

    public AccountId AccountId { get; private set; }

    public TransactionDescription Description { get; private set; } = null!;

    public TransactionDate Date { get; private set; } = null!;

    public TransactionId? RelatedTransactionId { get; private set; }

    public static Transaction Create(
      TransactionAmount amount,
      TransactionType type,
      TransactionDescription description,
      AccountId accountId,
      TransactionDate date,
      Guid createdBy)
    {
        return new Transaction(amount, type, description, accountId, date, createdBy);
    }

    public static Transaction Create(
        decimal amount,
        TransactionType type,
        string description, 
        Guid accountId,
        DateOnly date,
        Guid createdBy)
    {
        return Create(
            TransactionAmount.Create(amount),
            type,
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

        if (!TransactionAmountTypeRules.EnsureAmountMatchesType(Amount.Value, amount.Value, Type))
        {
            throw new InvalidOperationException($"Amount type '{amount.GetType().Name}' is not valid for transaction type '{Type.GetType().Name}'.");
        }

        Amount = amount;
        MarkAsModified(modifiedBy);
    }

    public void ChangeType(TransactionType type, Guid modifiedBy)
    {
        if (type == Type || type == TransactionType.Transfer)
        {
            return;
        }

        Type = type;
        ChangeAmount(Amount.Negate(), modifiedBy);
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

    public void ChangeRelatedTransactionId(TransactionId? relatedTransactionId)
    {
        RelatedTransactionId = relatedTransactionId;
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
