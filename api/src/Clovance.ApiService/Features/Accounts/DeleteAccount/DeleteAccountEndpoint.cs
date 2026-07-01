using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.DeleteAccount;

public class DeleteAccountEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id:guid}", async (
            Guid id, 
            IHandler<DeleteAccountCommand, Result> handler, 
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteAccountCommand(Id: id), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemResult(httpContext);
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("DeleteAccount")
        .WithSummary("Delete Account")
        .WithDescription("Deletes an existing account identified by its ID.");
    }
}
