namespace Clovance.ApiService.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/logout", async (LogoutCommandHandler handler) =>
        {
            await handler.Handle(new LogoutCommand());
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithTags("Auth");

        return builder;
    }
}
