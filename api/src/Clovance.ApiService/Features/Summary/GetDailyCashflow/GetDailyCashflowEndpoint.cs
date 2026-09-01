using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetDailyCashflow;

public class GetDailyCashflowEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/daily-cashflow", async (
            string currency,
            Guid? accountId,
            int? month,
            int? year,
            string? accountType,
            IHandler<GetDailyCashflowQuery, Result<GetDailyCashflowResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            AccountTypeFilter? parsedAccountType = string.IsNullOrWhiteSpace(accountType)
                ? null
                : Enum.TryParse<AccountTypeFilter>(accountType, ignoreCase: true, out var parsed)
                    ? parsed
                    : null;

            var query = new GetDailyCashflowQuery(
                AccountId: accountId,
                Month: month,
                Year: year,
                Currency: currency,
                AccountType: parsedAccountType);

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
        .WithDescription("Get Daily Cashflow for a given month/year with optional account filter and account type filter (asset or liability), converted to the specified currency.");
    }
}
