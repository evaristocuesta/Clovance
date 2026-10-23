namespace Clovance.ApiService.Infrastructure.RateLimiting;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public AuthRateLimitOptions Auth { get; set; } = new();
}

public sealed class AuthRateLimitOptions
{
    public bool Enabled { get; set; } = true;

    public int PermitLimit { get; set; } = 10;

    public int WindowSeconds { get; set; } = 60;
}
