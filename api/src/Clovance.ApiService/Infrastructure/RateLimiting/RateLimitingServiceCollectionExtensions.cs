using System.Diagnostics;
using System.Threading.RateLimiting;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Infrastructure.RateLimiting;

public static class RateLimitingServiceCollectionExtensions
{
    public const string AuthPolicyName = "auth";

    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(
            configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;

                if (httpContext.Response.HasStarted)
                {
                    return;
                }

                var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
                var status = StatusCodes.Status429TooManyRequests;

                var problemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = "Too Many Requests",
                    Detail = "Too many requests. Please try again later.",
                    Instance = httpContext.Request.Path,
                    Type = $"https://httpstatuses.com/{status}",
                    Extensions =
                    {
                        ["traceId"] = traceId,
                        ["errorCode"] = ErrorCodes.Common.TooManyRequests
                    }
                };

                httpContext.Response.StatusCode = status;
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };

            options.AddPolicy(AuthPolicyName, httpContext =>
            {
                var authOptions = httpContext.RequestServices
                    .GetRequiredService<IOptions<RateLimitingOptions>>()
                    .Value.Auth;

                var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return authOptions.Enabled
                    ? RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => CreateAuthFixedWindowOptions(authOptions))
                    : RateLimitPartition.GetNoLimiter(partitionKey);
            });
        });

        return services;
    }

    public static FixedWindowRateLimiterOptions CreateAuthFixedWindowOptions(AuthRateLimitOptions options) =>
        new()
        {
            PermitLimit = options.PermitLimit,
            Window = TimeSpan.FromSeconds(options.WindowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        };
}
