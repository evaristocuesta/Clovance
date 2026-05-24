using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/invitations", async (
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
        .Produces<CreateInvitationResult>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateInvitation")
        .WithSummary("Create Invitation")
        .WithDescription("Creates an invitation for a new user. Only users with the 'Admin' role can create invitations.");
    }
}
