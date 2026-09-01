using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetMonthlyBalance;

public class GetMonthlyBalanceEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/monthly-balance", async (
            string currency,
            Guid? accountId,
            int? month,
            int? year,
            string? accountType,
            IHandler<GetMonthlyBalanceQuery, Result<GetMonthlyBalanceResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            AccountTypeFilter? parsedAccountType = string.IsNullOrWhiteSpace(accountType)
                ? null
                : Enum.TryParse<AccountTypeFilter>(accountType, ignoreCase: true, out var parsed)
                    ? parsed
                    : null;

            var query = new GetMonthlyBalanceQuery(
                AccountId: accountId,
                Currency: currency,
                AnchorMonth: month,
                AnchorYear: year,
                AccountType: parsedAccountType);

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
        .WithDescription("Get Monthly Balance for a given month/year with optional account filter and account type filter (asset or liability), converted to the specified currency.");
    }
}
