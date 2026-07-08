using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.GetTransactionById;

public class GetTransactionByIdEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", async (
            Guid id,
            IHandler<GetTransactionByIdQuery, Result<GetTransactionByIdResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetTransactionByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value.Transaction) : result.ToProblemResult(httpContext);
        })
        .Produces<TransactionDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetTransactionById")
        .WithSummary("Get transaction by ID")
        .WithDescription("Retrieves a transaction by its unique identifier.");
    }
}
