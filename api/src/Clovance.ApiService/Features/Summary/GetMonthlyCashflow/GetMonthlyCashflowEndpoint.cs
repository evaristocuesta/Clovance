using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetMonthlyCashflow;

public class GetMonthlyCashflowEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/monthly-cashflow", async (
            string currency,
            Guid? accountId,
            int? month,
            int? year,
            string? accountType,
            IHandler<GetMonthlyCashflowQuery, Result<GetMonthlyCashflowResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            AccountTypeFilter? parsedAccountType = string.IsNullOrWhiteSpace(accountType)
                ? null
                : Enum.TryParse<AccountTypeFilter>(accountType, ignoreCase: true, out var parsed)
                    ? parsed
                    : null;

            var query = new GetMonthlyCashflowQuery(
                AccountId: accountId,
                Currency: currency,
                AnchorYear: year,
                AnchorMonth: month,
                AccountType: parsedAccountType);

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
        .WithDescription("Get Monthly Cashflow for a given month/year with optional account filter and account type filter (asset or liability), converted to the specified currency.");
    }
}
