using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.RestoreAccount;

public class RestoreAccountEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:guid}/restore", async (Guid id, IHandler<RestoreAccountCommand, Result> handler, CancellationToken cancellationToken) =>
        {
            var command = new RestoreAccountCommand(id);
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(result);
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("RestoreAccount")
        .WithSummary("Restore Account")
        .WithDescription("Restores a previously deleted account.");
    }
}
