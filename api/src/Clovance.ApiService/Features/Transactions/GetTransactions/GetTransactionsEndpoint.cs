using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.GetTransactions;

public class GetTransactionsEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            DateOnly? dateFrom,
            DateOnly? dateTo,
            Guid? accountId,
            string? description,
            DateOnly? cursorDate,
            Guid? cursorId,
            int? pageSize,
            IHandler<GetTransactionsQuery, Result<GetTransactionsPageResponse>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTransactionsQuery(
                dateFrom,
                dateTo,
                accountId,
                description,
                cursorDate,
                cursorId,
                pageSize ?? 30);

            var result = await handler.HandleAsync(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemResult(httpContext);
        })
        .Produces<GetTransactionsPageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithName("GetTransactions")
        .WithSummary("Get Transactions")
        .WithDescription("Get a paginated list of transactions with optional filtering by year, month, account ID, and description.");
    }
}
