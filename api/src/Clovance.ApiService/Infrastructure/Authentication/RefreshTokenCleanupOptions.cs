namespace Clovance.ApiService.Infrastructure.Authentication;

public sealed class RefreshTokenCleanupOptions
{
    public const string SectionName = "Identity:RefreshTokens:Cleanup";

    public bool Enabled { get; init; } = true;

    public int IntervalHours { get; init; } = 24;

    public int BatchSize { get; init; } = 500;

    public int UsedRetentionHours { get; init; } = 48;
}
