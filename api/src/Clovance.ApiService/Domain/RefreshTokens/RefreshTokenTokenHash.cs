using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.RefreshTokens;

public sealed class RefreshTokenTokenHash : ValueObject
{
    private RefreshTokenTokenHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RefreshTokenTokenHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Refresh token is required.", nameof(value));
        }

        var normalizedToken = value
            .Trim();

        if (normalizedToken.Length != 64)
        {
            throw new ArgumentException("Refresh token must be 64 characters long.", nameof(value));
        }

        return new RefreshTokenTokenHash(normalizedToken);
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
