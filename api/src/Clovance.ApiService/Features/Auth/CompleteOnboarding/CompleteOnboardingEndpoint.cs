using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed class CompleteOnboardingEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth");

        authGroup.MapPost("/complete-onboarding", async (
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
        .RequireAuthorization()
        .WithName("CompleteOnboarding")
        .WithTags("Auth");
    }
}
