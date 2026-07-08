using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Transactions;

public sealed class TransactionDate : ValueObject, IComparable<TransactionDate>
{
    private TransactionDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static TransactionDate Create(DateOnly value)
    {
        return new TransactionDate(value);
    }

    public int CompareTo(TransactionDate? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString("O");
    }

    public static bool operator <(TransactionDate? left, TransactionDate? right)
    {
        if (left is null) return right is not null;
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(TransactionDate? left, TransactionDate? right)
    {
        if (left is null) return false;
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(TransactionDate? left, TransactionDate? right)
    {
        if (left is null) return true;
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(TransactionDate? left, TransactionDate? right)
    {
        if (left is null) return right is null;
        return left.CompareTo(right) >= 0;
    }
}
