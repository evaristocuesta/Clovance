using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed class CompleteOnboardingEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/complete-onboarding", async (
            CompleteOnboardingCommand command, 
            IHandler<CompleteOnboardingCommand, Unit> handler, 
            CancellationToken cancellationToken) =>
        {
            try
            {
                await handler.HandleAsync(command, cancellationToken);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Error"] = [ex.Message]
                });
            }
        })
        .WithValidation<CompleteOnboardingCommand>()
        .Produces<Unit>(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("CompleteOnboarding")
        .WithSummary("Complete Onboarding")
        .WithDescription("Completes the onboarding process for first admin user. First admin user must change the email and password.");
    }
}
