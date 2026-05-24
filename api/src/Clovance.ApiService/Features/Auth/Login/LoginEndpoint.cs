using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
            LoginCommand command, 
            IHandler<LoginCommand, LoginResult> handler, 
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(new
            {
                access_token = result.AccessToken,
                token_type = "Bearer",
                expires_at = result.ExpiresAt
            });
        })
        .WithValidation<LoginCommand>()
        .Produces<LoginResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithName("Login")
        .WithSummary("Login")
        .WithDescription("Returns a Bearer token in the response that can be used for authenticated requests.");
    }
}
