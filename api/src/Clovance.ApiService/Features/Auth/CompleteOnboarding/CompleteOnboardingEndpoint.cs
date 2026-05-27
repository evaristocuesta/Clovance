using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed class CompleteOnboardingEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/complete-onboarding", async (
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
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("CompleteOnboarding")
        .WithSummary("Complete Onboarding")
        .WithDescription("Completes the onboarding process for first admin user. First admin user must change the email and password.");
    }
}
