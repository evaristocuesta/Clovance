namespace Clovance.ApiService.Domain.RefreshTokens;

public readonly record struct RefreshTokenId(Guid Value)
{
    public static RefreshTokenId New()
    {
        return new RefreshTokenId(Guid.NewGuid());
    }

    public static RefreshTokenId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Refresh token id cannot be empty.", nameof(value));
        }
        return new RefreshTokenId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
