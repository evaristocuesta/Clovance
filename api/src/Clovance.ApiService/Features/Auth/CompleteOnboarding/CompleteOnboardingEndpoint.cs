using Clovance.ApiService.Infrastructure.Validation;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public static class CompleteOnboardingEndpoint
{
    public static IEndpointRouteBuilder MapCompleteOnboardingEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/complete-onboarding", async (CompleteOnboardingCommand command, CompleteOnboardingCommandHandler handler) =>
        {
            try
            {
                await handler.Handle(command);
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

        return builder;
    }
}
