using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.RefreshTokens;

public class RefreshToken : EntityBase<RefreshTokenId>
{
    private RefreshToken()
    {
    }
    private RefreshToken(RefreshTokenId id, Guid userId, string tokenHash, DateTimeOffset expiresAt, bool isUsed)
    {
        Id = id;
        UserId = RefreshTokenUserId.Create(userId);
        TokenHash = RefreshTokenTokenHash.Create(tokenHash);
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
        IsUsed = isUsed;
    }

    public RefreshTokenUserId UserId { get; private set; } = null!;

    public RefreshTokenTokenHash TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsUsed { get; private set; }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAt)
    {
        return new RefreshToken(RefreshTokenId.New(), userId, tokenHash, expiresAt, false);
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
