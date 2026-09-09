using System.Security.Cryptography;
using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.PasswordResetTokens;

public class PasswordResetToken : EntityBase<PasswordResetTokenId>
{
    private PasswordResetToken()
    {
    }

    private PasswordResetToken(
        PasswordResetTokenId id,
        Guid userId,
        PasswordResetTokenHash tokenHash,
        DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = PasswordResetTokenUserId.Create(userId);
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
        IsUsed = false;
    }

    public PasswordResetTokenUserId UserId { get; private set; } = null!;

    public PasswordResetTokenHash TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsUsed { get; private set; }

    public static (PasswordResetToken Token, string PlainTextToken) Create(Guid userId, TimeSpan expiration)
    {
        var plainTextToken = GeneratePlainTextToken();
        var tokenHash = PasswordResetTokenHash.FromPlainTextToken(plainTextToken);

        var token = new PasswordResetToken(
            PasswordResetTokenId.New(),
            userId,
            tokenHash,
            DateTimeOffset.UtcNow.Add(expiration));

        return (token, plainTextToken);
    }

    public bool IsValid(DateTimeOffset utcNow)
    {
        return !IsUsed && ExpiresAt > utcNow;
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Password reset token has already been used.");
        }

        IsUsed = true;
    }

    private static string GeneratePlainTextToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_'); // URL-safe
    }
}
