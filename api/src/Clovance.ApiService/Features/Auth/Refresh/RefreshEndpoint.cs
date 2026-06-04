using Clovance.ApiService.Features.Shared;

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

            return Results.Ok(result.Value);
        })
        .Produces<RefreshResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("Refresh")
        .WithSummary("Refresh")
        .WithDescription("Refresh the authentication token");
    }
}
