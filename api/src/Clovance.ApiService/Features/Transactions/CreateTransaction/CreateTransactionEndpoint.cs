using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.CreateTransaction;

public class CreateTransactionEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateTransactionCommand command, 
            IHandler<CreateTransactionCommand, Result<CreateTransactionResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            
            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(result.Value.Transaction);
        })
        .Produces<TransactionDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization()
        .WithName("CreateTransaction")
        .WithSummary("Create Transaction")
        .WithDescription("Creates a new transaction");
    }
}
