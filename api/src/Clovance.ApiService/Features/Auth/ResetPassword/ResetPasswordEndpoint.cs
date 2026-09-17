using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.ResetPassword;

public sealed class ResetPasswordEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/reset-password", async (
            ResetPasswordCommand command,
            IHandler<ResetPasswordCommand, Result> handler,
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
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous()
        .WithName("ResetPassword")
        .WithSummary("Reset password")
        .WithDescription("Reset a user's password using a password reset token");
    }
}
