using System.Diagnostics;
using System.Security.Claims;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
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

            if (path.StartsWithSegments("/api/auth/complete-onboarding", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var requiresAuth = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>()?.Count > 0;
            var isAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null;

            if (!requiresAuth || isAnonymous)
            {
                await next();
                return;
            }

            var mustCompleteOnBoardingClaim = context.User.FindFirstValue(JwtTokenService.MUST_COMPLETE_ONBOARDING);
            var mustCompleteOnBoarding = string.Equals(mustCompleteOnBoardingClaim, bool.TrueString, StringComparison.OrdinalIgnoreCase);

            if (mustCompleteOnBoarding)
            {
                var error = AppErrors.Auth.MustCompleteOnBoarding();
                var result = Result.Failure(error);
                await result.ToProblemResult(context).ExecuteAsync(context);
                return;
            }

            await next();
        });

        return app;
    }
}
