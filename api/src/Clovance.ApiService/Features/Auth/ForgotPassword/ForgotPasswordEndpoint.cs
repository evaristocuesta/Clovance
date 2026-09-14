using Clovance.ApiService.Features.Shared;

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
        .AllowAnonymous()
        .WithName("ForgotPassword")
        .WithSummary("Forgot password")
        .WithDescription("Request a password reset email");
    }
}
