namespace Clovance.ApiService.Infrastructure.Auth.PasswordReset;

public sealed class PasswordResetOptions
{
    public const string SectionName = "Identity:PasswordReset";

    public int ExpirationMinutes { get; init; } = 30;
}
