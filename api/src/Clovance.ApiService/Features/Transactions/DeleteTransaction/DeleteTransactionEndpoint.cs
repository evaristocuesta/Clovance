using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.DeleteTransaction;

public class DeleteTransactionEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id:guid}", async (
            Guid id, 
            IHandler<DeleteTransactionCommand, Result> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteTransactionCommand(id);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemResult(httpContext);
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("DeleteTransaction")
        .WithSummary("Delete Transaction")
        .WithDescription("Deletes an existing transaction identified by its ID.");
    }
}
