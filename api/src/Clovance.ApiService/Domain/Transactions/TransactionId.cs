namespace Clovance.ApiService.Domain.Transactions;

public readonly record struct TransactionId(Guid Value) : IComparable<TransactionId>
{
    public static TransactionId New()
    {
        return new TransactionId(Guid.CreateVersion7());
    }

    public static TransactionId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Transaction id cannot be empty.", nameof(value));
        }

        return new TransactionId(value);
    }

    public int CompareTo(TransactionId other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static bool operator <(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) >= 0;
    }
}
