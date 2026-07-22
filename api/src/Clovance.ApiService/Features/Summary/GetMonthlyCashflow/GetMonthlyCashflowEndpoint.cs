using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Summary.GetMonthlyCashflow;

public class GetMonthlyCashflowEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/monthly-cashflow", async (
            Guid? accountId,
            int? month,
            int? year,
            IHandler<GetMonthlyCashflowQuery, Result<GetMonthlyCashflowResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMonthlyCashflowQuery(
                AccountId: accountId,
                AnchorYear: year,
                AnchorMonth: month);

            var result = await handler.HandleAsync(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemResult(httpContext);
        })
        .Produces<GetMonthlyCashflowResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization()
        .WithName("GetMonthlyCashflow")
        .WithSummary("Get Monthly Cashflow")
        .WithDescription("Get Monthly Cashflow for a given month/year with optional account filter.");
    }
}
