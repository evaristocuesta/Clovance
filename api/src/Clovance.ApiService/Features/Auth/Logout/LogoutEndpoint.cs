namespace Clovance.ApiService.Features.Auth.Logout;

using Clovance.ApiService.Features.Shared;

public sealed class LogoutEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth");

        authGroup.MapPost("/logout", async (
            IHandler<LogoutCommand, Unit> handler, 
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new LogoutCommand(), cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithTags("Auth");
    }
}
