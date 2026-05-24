using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth");

        authGroup.MapPost("/invitations", async (
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
    }
}
