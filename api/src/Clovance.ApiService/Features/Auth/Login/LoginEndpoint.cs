using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth");

        authGroup.MapPost("/login", async (
            LoginCommand command, 
            IHandler<LoginCommand, LoginResult> handler, 
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Ok(new
                {
                    access_token = result.AccessToken,
                    token_type = "Bearer",
                    expires_at = result.ExpiresAt
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithValidation<LoginCommand>()
        .WithName("Login")
        .WithTags("Auth")
        .WithSummary("Login to get Bearer token")
        .WithDescription("Returns a Bearer token in the response that can be used for authenticated requests. Copy the 'access_token' value from the response.");
    }
}
