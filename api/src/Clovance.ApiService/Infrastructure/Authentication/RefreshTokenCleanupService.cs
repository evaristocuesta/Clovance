using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Infrastructure.Authentication;

public sealed class RefreshTokenCleanupService : BackgroundService
{
    private readonly IDbContextFactory<ClovanceDbContext> _dbContextFactory;
    private readonly RefreshTokenCleanupOptions _options;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IDbContextFactory<ClovanceDbContext> dbContextFactory,
        IOptions<RefreshTokenCleanupOptions> options,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Refresh token cleanup is disabled.");
            return;
        }

        var interval = TimeSpan.FromHours(Math.Max(1, _options.IntervalHours));

        await CleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var usedTokenCutoff = now.AddHours(-Math.Max(0, _options.UsedRetentionHours));
        var batchSize = Math.Max(1, _options.BatchSize);
        var totalDeleted = 0;

        while (true)
        {
            var deletedCount = await dbContext
                .RefreshTokens
                .Where(token => token.ExpiresAt <= now || (token.IsUsed && token.CreatedAt <= usedTokenCutoff))
                .OrderBy(token => token.ExpiresAt)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);

            totalDeleted += deletedCount;

            if (deletedCount < batchSize)
            {
                break;
            }
        }

        if (totalDeleted > 0)
        {
            _logger.LogInformation(
                "Refresh token cleanup removed {DeletedCount} rows in batches of {BatchSize}.",
                totalDeleted,
                batchSize);
        }
    }
}
