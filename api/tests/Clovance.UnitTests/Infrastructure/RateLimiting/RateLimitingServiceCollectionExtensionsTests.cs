using System.Threading.RateLimiting;
using Clovance.ApiService.Infrastructure.RateLimiting;

namespace Clovance.UnitTests.Infrastructure.RateLimiting;

public class RateLimitingServiceCollectionExtensionsTests
{
    [Fact]
    public void AuthRateLimitOptions_Defaults_ProduceProductionSafeLimits()
    {
        var options = new AuthRateLimitOptions();

        Assert.True(options.Enabled);
        Assert.Equal(10, options.PermitLimit);
        Assert.Equal(60, options.WindowSeconds);
    }

    [Fact]
    public void CreateAuthFixedWindowOptions_MapsConfiguredValues()
    {
        var options = new AuthRateLimitOptions { PermitLimit = 3, WindowSeconds = 30 };

        var limiterOptions = RateLimitingServiceCollectionExtensions.CreateAuthFixedWindowOptions(options);

        Assert.Equal(3, limiterOptions.PermitLimit);
        Assert.Equal(TimeSpan.FromSeconds(30), limiterOptions.Window);
        Assert.Equal(0, limiterOptions.QueueLimit);
        Assert.True(limiterOptions.AutoReplenishment);
    }

    [Fact]
    public async Task FixedWindowLimiter_RejectsRequestsOverPermitLimit()
    {
        var limiterOptions = RateLimitingServiceCollectionExtensions.CreateAuthFixedWindowOptions(
            new AuthRateLimitOptions { PermitLimit = 3, WindowSeconds = 60 });

        using var limiter = new FixedWindowRateLimiter(limiterOptions);

        for (var i = 0; i < 3; i++)
        {
            using var lease = await limiter.AcquireAsync(1, TestContext.Current.CancellationToken);
            Assert.True(lease.IsAcquired);
        }

        using var rejected = await limiter.AcquireAsync(1, TestContext.Current.CancellationToken);
        Assert.False(rejected.IsAcquired);
    }
}
