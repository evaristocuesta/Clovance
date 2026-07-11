using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.UpdateTransaction;

public class UpdateTransactionEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:guid}", async (
            UpdateTransactionRequest request, 
            IHandler<UpdateTransactionCommand, Result<UpdateTransactionResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken, 
            Guid id) =>
        {
            var command = new UpdateTransactionCommand(
                Transaction: new TransactionDto(
                    Id: id,
                    Date: request.Date,
                    Description: request.Description,
                    Amount: request.Amount,
                    Type: request.Type,
                    AccountId: request.AccountId,
                    AccountName: string.Empty,
                    RelatedTransactionId: null
                )
            );

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value.Transaction) : result.ToProblemResult(httpContext);
        })
        .Produces<TransactionDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("UpdateTransaction")
        .WithSummary("Update Transaction")
        .WithDescription("Updates an existing transaction identified by its ID.");
    }
}
