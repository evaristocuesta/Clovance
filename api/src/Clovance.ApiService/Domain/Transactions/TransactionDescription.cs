using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Transactions;

public sealed class TransactionDescription : ValueObject
{
    private TransactionDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TransactionDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Transaction description is required.", nameof(value));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > 250)
        {
            throw new ArgumentException("Transaction description cannot exceed 250 characters.", nameof(value));
        }

        return new TransactionDescription(normalizedValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
