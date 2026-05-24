namespace Clovance.ApiService.Features.Auth.Logout;

using Clovance.ApiService.Features.Shared;

public sealed class LogoutEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
            IHandler<LogoutCommand, Unit> handler, 
            CancellationToken cancellationToken) =>
        {
            try
            {
                await handler.HandleAsync(new LogoutCommand(), cancellationToken);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .RequireAuthorization()
        .Produces<Unit>(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithName("Logout")
        .WithSummary("Logout")
        .WithDescription("Logs out the current user and invalidates the Bearer token.");
    }
}
