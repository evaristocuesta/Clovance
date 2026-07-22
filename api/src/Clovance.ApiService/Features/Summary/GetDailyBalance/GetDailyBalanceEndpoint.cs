using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Summary.GetDailyBalance;

public class GetDailyBalanceEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/daily-balance", async (
            Guid? accountId,
            int? month,
            int? year,
            IHandler<GetDailyBalanceQuery, Result<GetDailyBalanceResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDailyBalanceQuery(
                AccountId: accountId,
                Month: month,
                Year: year);

            var result = await handler.HandleAsync(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemResult(httpContext);
        })
        .Produces<GetDailyBalanceResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetDailyBalance")
        .WithSummary("Get Daily Balance")
        .WithDescription("Get Daily Balance for a given month/year with optional account filter.");
    }
}
