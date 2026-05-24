using Clovance.ApiService.Infrastructure.Validation;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public static class CreateInvitationEndpoint
{
    public static IEndpointRouteBuilder MapCreateInvitationEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/invitations", async (CreateInvitationCommand command, CreateInvitationCommandHandler handler) =>
        {
            try
            {
                var result = await handler.Handle(command);
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
