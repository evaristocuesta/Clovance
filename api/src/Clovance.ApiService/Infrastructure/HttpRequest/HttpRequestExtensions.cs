using System.Diagnostics;

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
}
