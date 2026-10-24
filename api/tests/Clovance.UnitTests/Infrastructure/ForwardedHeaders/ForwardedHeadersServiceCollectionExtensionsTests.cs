using System.Net;
using Clovance.ApiService.Infrastructure.ForwardedHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using ForwardedHeadersFlags = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders;

namespace Clovance.UnitTests.Infrastructure.ForwardedHeaders;

public class ForwardedHeadersServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureOptions_WhenDisabled_LeavesForwardedHeadersUnprocessed()
    {
        var options = new ForwardedHeadersOptions();

        ForwardedHeadersServiceCollectionExtensions.ConfigureOptions(
            options,
            BuildConfiguration(new Dictionary<string, string?>()));

        Assert.Equal(ForwardedHeadersFlags.None, options.ForwardedHeaders);
    }

    [Fact]
    public void ConfigureOptions_EnabledInConfig_ForwardsProtoAndForFromTrustedSources()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:Enabled"] = "true",
            ["ForwardedHeaders:ForwardLimit"] = "2",
            ["ForwardedHeaders:KnownProxies:0"] = "203.0.113.10",
            ["ForwardedHeaders:KnownNetworks:0"] = "172.16.0.0/12"
        });

        var options = new ForwardedHeadersOptions();
        ForwardedHeadersServiceCollectionExtensions.ConfigureOptions(options, configuration);

        Assert.Equal(
            ForwardedHeadersFlags.XForwardedFor | ForwardedHeadersFlags.XForwardedProto,
            options.ForwardedHeaders);
        Assert.Equal(2, options.ForwardLimit);
        Assert.Contains(IPAddress.Parse("203.0.113.10"), options.KnownProxies);
        Assert.Contains(IPNetwork.Parse("172.16.0.0/12"), options.KnownIPNetworks);
    }

    [Fact]
    public void ConfigureOptions_EnabledThroughEnvironmentVariable_ForwardsHeaders()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] = "true"
        });

        var options = new ForwardedHeadersOptions();
        ForwardedHeadersServiceCollectionExtensions.ConfigureOptions(options, configuration);

        Assert.Equal(
            ForwardedHeadersFlags.XForwardedFor | ForwardedHeadersFlags.XForwardedProto,
            options.ForwardedHeaders);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
