using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public static class RegisterWithInvitationEndpoint
{
    public static IEndpointRouteBuilder MapRegisterWithInvitationEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/register-with-invitation", async (
            RegisterWithInvitationCommand command, 
            IHandler<RegisterWithInvitationCommand, RegisterWithInvitationResult> handler, 
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Created($"/auth/users/{result.UserId}", result);
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
        .WithValidation<RegisterWithInvitationCommand>()
        .WithName("RegisterWithInvitation")
        .WithTags("Auth");

        return builder;
    }
}
