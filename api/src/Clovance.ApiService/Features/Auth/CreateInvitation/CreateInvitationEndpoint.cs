using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public static class CreateInvitationEndpoint
{
    public static IEndpointRouteBuilder MapCreateInvitationEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/invitations", async (
            CreateInvitationCommand command, 
            IHandler<CreateInvitationCommand, CreateInvitationResult> handler, 
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Created($"/auth/invitations/{result.Id}", result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Email"] = [ex.Message]
                });
            }
        })
        .WithValidation<CreateInvitationCommand>()
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateInvitation")
        .WithTags("Auth");

        return builder;
    }
}
