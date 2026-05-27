using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/invitations", async (
            CreateInvitationCommand command, 
            IHandler<CreateInvitationCommand, Result<CreateInvitationResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Created($"/auth/invitations/{result.Value.Id}", result.Value);
        })
        .Produces<CreateInvitationResult>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateInvitation")
        .WithSummary("Create Invitation")
        .WithDescription("Creates an invitation for a new user. Only users with the 'Admin' role can create invitations.");
    }
}
