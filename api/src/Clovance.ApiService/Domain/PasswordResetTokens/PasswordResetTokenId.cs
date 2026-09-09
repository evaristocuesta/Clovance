namespace Clovance.ApiService.Domain.PasswordResetTokens;

public readonly record struct PasswordResetTokenId(Guid Value)
{
    public static PasswordResetTokenId New()
    {
        return new PasswordResetTokenId(Guid.CreateVersion7());
    }

    public static PasswordResetTokenId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Password reset token id cannot be empty.", nameof(value));
        }
        return new PasswordResetTokenId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
