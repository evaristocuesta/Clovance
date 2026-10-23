using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.RateLimiting;

namespace Clovance.ApiService.Features.Auth.Refresh;

public sealed class RefreshEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh", async (
            RefreshCommand command,
            IHandler<RefreshCommand, Result<RefreshResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Created(string.Empty, result.Value);
        })
        .Produces<RefreshResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthPolicyName)
        .WithName("Refresh")
        .WithSummary("Refresh")
        .WithDescription("Refresh the authentication token");
    }
}
