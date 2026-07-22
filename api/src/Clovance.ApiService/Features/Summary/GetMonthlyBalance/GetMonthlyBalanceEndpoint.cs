using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Summary.GetMonthlyBalance;

public class GetMonthlyBalanceEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/monthly-balance", async (
            Guid? accountId,
            int? month,
            int? year,
            IHandler<GetMonthlyBalanceQuery, Result<GetMonthlyBalanceResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMonthlyBalanceQuery(
                AccountId: accountId,
                AnchorMonth: month,
                AnchorYear: year);

            var result = await handler.HandleAsync(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemResult(httpContext);
        })
        .Produces<GetMonthlyBalanceResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetMonthlyBalance")
        .WithSummary("Get Monthly Balance")
        .WithDescription("Get Monthly Balance for a given month/year with optional account filter.");
    }
}
