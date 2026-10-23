using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.RateLimiting;

namespace Clovance.ApiService.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/forgot-password", async (
            ForgotPasswordCommand command,
            IHandler<ForgotPasswordCommand, Result> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthPolicyName)
        .WithName("ForgotPassword")
        .WithSummary("Forgot password")
        .WithDescription("Request a password reset email");
    }
}
