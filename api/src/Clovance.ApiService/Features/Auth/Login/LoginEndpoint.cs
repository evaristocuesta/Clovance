using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
            LoginCommand command, 
            IHandler<LoginCommand, Result<LoginResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(new LoginResponse(
                result.Value.AccessToken,
                "Bearer",
                result.Value.ExpiresAt));
        })
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithName("Login")
        .WithSummary("Login")
        .WithDescription("Returns a Bearer token in the response that can be used for authenticated requests.");
    }
}
