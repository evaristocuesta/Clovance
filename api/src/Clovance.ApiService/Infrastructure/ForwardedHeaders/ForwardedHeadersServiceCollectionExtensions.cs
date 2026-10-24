using System.Net;
using ForwardedHeadersFlags = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders;

namespace Clovance.ApiService.Infrastructure.ForwardedHeaders;

public static class ForwardedHeadersServiceCollectionExtensions
{
    private const string EnabledKey = "ForwardedHeaders:Enabled";
    private const string EnvVarEnabledKey = "ASPNETCORE_FORWARDEDHEADERS_ENABLED";

    public static IServiceCollection AddForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(
            options => ConfigureOptions(options, configuration));

        return services;
    }

    /// <summary>
    /// Applies the forwarded headers configuration. Headers are only processed when enabled
    /// through the ForwardedHeaders section or the ASPNETCORE_FORWARDEDHEADERS_ENABLED
    /// environment variable, and only when they come from the explicitly trusted proxies
    /// or networks (otherwise X-Forwarded-* headers would be spoofable by any client).
    /// </summary>
    public static void ConfigureOptions(
        ForwardedHeadersOptions options,
        IConfiguration configuration)
    {
        if (!IsEnabled(configuration))
        {
            return;
        }

        options.ForwardedHeaders = ForwardedHeadersFlags.XForwardedFor | ForwardedHeadersFlags.XForwardedProto;
        options.ForwardLimit = configuration.GetValue("ForwardedHeaders:ForwardLimit", 1);

        var knownProxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]?>();

        foreach (var proxy in knownProxies ?? [])
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }

        var knownNetworks = configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]?>();

        foreach (var network in knownNetworks ?? [])
        {
            options.KnownIPNetworks.Add(IPNetwork.Parse(network));
        }
    }

    public static bool IsEnabled(IConfiguration configuration) =>
        configuration.GetValue(EnabledKey, false)
        || string.Equals(configuration[EnvVarEnabledKey], "true", StringComparison.OrdinalIgnoreCase);
}
