using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.RefreshTokens;

public sealed class RefreshTokenToken : ValueObject
{
    private RefreshTokenToken(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RefreshTokenToken Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Refresh token is required.", nameof(value));
        }

        var normalizedToken = value
            .Trim();

        if (normalizedToken.Length > 200)
        {
            throw new ArgumentException("Refresh token is too long.", nameof(value));
        }

        return new RefreshTokenToken(normalizedToken);
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
