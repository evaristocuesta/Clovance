using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Clovance.ApiService.Infrastructure.HttpRequest;

public static class HttpRequestExtensions
{
    public static IApplicationBuilder UseHttpRequestTracing(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("HttpRequestTrace");

            var stopwatch = Stopwatch.StartNew();
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            logger.LogInformation(
                "HTTP request started. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                traceId);

            await next();

            stopwatch.Stop();

            logger.LogInformation(
                "HTTP request completed. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, ElapsedMs: {ElapsedMs}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                traceId);
        });

        return app;
    }

    public static IApplicationBuilder UseOnboardingEnforcement(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await next();
                return;
            }

            var endpoint = context.GetEndpoint();
            var path = context.Request.Path;

            if (path.StartsWithSegments("/auth/complete-onboarding", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null)
            {
                await next();
                return;
            }

            var mustChangePasswordClaim = context.User.FindFirstValue("must_complete_onboarding");
            var mustChangePassword = string.Equals(mustChangePasswordClaim, bool.TrueString, StringComparison.OrdinalIgnoreCase);

            if (mustChangePassword)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "password_change_required",
                    detail = "You must change your password before accessing protected endpoints."
                });
                return;
            }

            await next();
        });

        return app;
    }
}
