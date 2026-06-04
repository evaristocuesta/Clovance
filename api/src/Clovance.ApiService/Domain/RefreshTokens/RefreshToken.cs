using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.RefreshTokens;

public class RefreshToken : EntityBase<RefreshTokenId>
{
    private RefreshToken()
    {
    }
    private RefreshToken(RefreshTokenId id, string userId, string token, DateTimeOffset expiresAt, bool isUsed)
    {
        Id = id;
        UserId = RefreshTokenUserId.Create(userId);
        Token = RefreshTokenToken.Create(token);
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
        IsUsed = isUsed;
    }

    public RefreshTokenUserId UserId { get; private set; } = null!;

    public RefreshTokenToken Token { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsUsed { get; private set; }

    public static RefreshToken Create(string userId, string token, DateTimeOffset expiresAt)
    {
        return new RefreshToken(RefreshTokenId.New(), userId, token, expiresAt, false);
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Refresh token has already been used.");
        }

        IsUsed = true;
    }
}
