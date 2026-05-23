namespace Clovance.ApiService.Domain.Transactions;

public readonly record struct TransactionId(Guid Value)
{
  public static TransactionId New()
  {
    return new TransactionId(Guid.NewGuid());
  }

  public static TransactionId Create(Guid value)
  {
    if (value == Guid.Empty)
    {
      throw new ArgumentException("Transaction id cannot be empty.", nameof(value));
    }

    return new TransactionId(value);
  }

  public override string ToString()
  {
    return Value.ToString();
  }
}
