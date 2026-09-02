using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Infrastructure.Authentication;

public static class RefreshTokenCleanupServiceCollectionExtensions
{
    public static IServiceCollection AddRefreshTokenCleanup(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<RefreshTokenCleanupOptions>()
            .Bind(configuration.GetSection(RefreshTokenCleanupOptions.SectionName))
            .Validate(
                options => options.IntervalHours > 0,
                $"{RefreshTokenCleanupOptions.SectionName}:IntervalHours must be greater than 0.")
            .Validate(
                options => options.BatchSize > 0,
                $"{RefreshTokenCleanupOptions.SectionName}:BatchSize must be greater than 0.")
            .Validate(
                options => options.UsedRetentionHours >= 0,
                $"{RefreshTokenCleanupOptions.SectionName}:UsedRetentionHours must be greater than or equal to 0.")
            .ValidateOnStart();

        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }
}
