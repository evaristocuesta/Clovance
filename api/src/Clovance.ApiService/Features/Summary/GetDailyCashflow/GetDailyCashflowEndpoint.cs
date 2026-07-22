using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Summary.GetDailyCashflow;

public class GetDailyCashflowEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/daily-cashflow", async (
            Guid? accountId,
            int? month,
            int? year,
            IHandler<GetDailyCashflowQuery, Result<GetDailyCashflowResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDailyCashflowQuery(
                AccountId: accountId,
                Month: month,
                Year: year);

            var result = await handler.HandleAsync(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemResult(httpContext);
        })
        .Produces<GetDailyCashflowResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetDailyCashflow")
        .WithSummary("Get Daily Cashflow")
        .WithDescription("Get Daily Cashflow for a given month/year with optional account filter.");
    }
}
