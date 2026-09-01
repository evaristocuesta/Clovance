using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetDailyBalance;

public class GetDailyBalanceEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/daily-balance", async (
            string currency,
            Guid? accountId,
            int? month,
            int? year,
            string? accountType,
            IHandler<GetDailyBalanceQuery, Result<GetDailyBalanceResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            AccountTypeFilter? parsedAccountType = string.IsNullOrWhiteSpace(accountType)
                ? null
                : Enum.TryParse<AccountTypeFilter>(accountType, ignoreCase: true, out var parsed)
                    ? parsed
                    : null;

            var query = new GetDailyBalanceQuery(
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
        .Produces<GetDailyBalanceResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetDailyBalance")
        .WithSummary("Get Daily Balance")
        .WithDescription("Get Daily Balance for a given month/year with optional account filter and account type filter (asset or liability), converted to the specified currency.");
    }
}
