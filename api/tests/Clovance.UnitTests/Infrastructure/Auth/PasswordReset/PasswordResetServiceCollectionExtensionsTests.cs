using Clovance.ApiService.Infrastructure.Auth.PasswordReset;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Clovance.UnitTests.Infrastructure.Auth.PasswordReset;

public class PasswordResetServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPasswordReset_WithoutConfiguration_UsesDefaultThirtyMinuteTokenLifespan()
    {
        var services = new ServiceCollection();

        services.AddPasswordReset(new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();
        var tokenProviderOptions = provider.GetRequiredService<IOptions<DataProtectionTokenProviderOptions>>();

        Assert.Equal(TimeSpan.FromMinutes(30), tokenProviderOptions.Value.TokenLifespan);
    }

    [Fact]
    public void AddPasswordReset_WithConfiguredExpiration_EnforcesItAsTokenLifespan()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Identity:PasswordReset:ExpirationMinutes"] = "5"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddPasswordReset(configuration);

        using var provider = services.BuildServiceProvider();
        var tokenProviderOptions = provider.GetRequiredService<IOptions<DataProtectionTokenProviderOptions>>();

        Assert.Equal(TimeSpan.FromMinutes(5), tokenProviderOptions.Value.TokenLifespan);
    }
}
