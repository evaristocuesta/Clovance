namespace Clovance.ApiService.Features.Auth.Logout;

using Clovance.ApiService.Features.Shared;

public sealed class LogoutEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
            IHandler<LogoutCommand, Result> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new LogoutCommand(), cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.NoContent();
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithName("Logout")
        .WithSummary("Logout")
        .WithDescription("Logs out the current user and invalidates the Bearer token.");
    }
}
