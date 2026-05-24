namespace Clovance.ApiService.Features.Auth.Logout;

using Clovance.ApiService.Features.Shared;

public static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/logout", async (
            IHandler<LogoutCommand, Unit> handler, 
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new LogoutCommand(), cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithTags("Auth");

        return builder;
    }
}
