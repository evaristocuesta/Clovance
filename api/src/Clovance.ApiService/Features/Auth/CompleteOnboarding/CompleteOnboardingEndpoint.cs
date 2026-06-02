using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed class CompleteOnboardingEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/complete-onboarding", async (
            CompleteOnboardingCommand command,
            IHandler<CompleteOnboardingCommand, Result> handler,
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
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CompleteOnboarding")
        .WithSummary("Complete Onboarding")
        .WithDescription("Completes the onboarding process for first admin user. First admin user must change the email and password.");
    }
}
