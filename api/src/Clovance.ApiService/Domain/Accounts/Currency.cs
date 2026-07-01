using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Accounts;

public sealed class Currency : ValueObject
{
    private Currency(string code)
    {
        Code = code;
    }

    public string Code { get; }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code is required.", nameof(code));
        }

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (!CurrencyValidator.IsValid(normalizedCode))
        {
            throw new ArgumentException($"Currency code '{normalizedCode}' is not valid.", nameof(code));
        }

        return new Currency(normalizedCode);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString()
    {
        return Code;
    }
}
