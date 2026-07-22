using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Summary.GetDailyBalance;

public class GetDailyBalanceQueryHandler : IHandler<GetDailyBalanceQuery, Result<GetDailyBalanceResult>>
{
    private readonly ClovanceDbContext _context;

    public GetDailyBalanceQueryHandler(ClovanceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetDailyBalanceResult>> HandleAsync(GetDailyBalanceQuery query, CancellationToken cancellationToken)
    {
        var monthStart = new DateOnly(query.Year ?? DateTime.UtcNow.Year, query.Month ?? DateTime.UtcNow.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var baseQuery = _context.Transactions
            .Where(t => query.AccountId == null || t.AccountId == AccountId.Create(query.AccountId.Value));

        var openingByAccount = await TransactionSummaryQueries.GetOpeningBalancesAsync(baseQuery, monthStart, cancellationToken);
        var dailyFlows = await TransactionSummaryQueries.GetDailyFlowsAsync(baseQuery, monthStart, monthEnd, cancellationToken);

        var netsByPeriod = dailyFlows
            .GroupBy(f => PeriodKey.Daily(f.Date))
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => (f.AccountId, f.Net)).ToList());

        var orderedPeriods = Enumerable.Range(0, monthEnd.DayNumber - monthStart.DayNumber + 1)
            .Select(i => PeriodKey.Daily(monthStart.AddDays(i)))
            .ToList();

        var rolled = BalanceRollForward.Calculate(
            orderedPeriods,
            openingByAccount,
            netsByPeriod,
            includeByAccount: query.AccountId is null);

        var result = rolled
            .Select(r => new DailyBalancePoint(new DateOnly(r.Period.Year, r.Period.Month, r.Period.Day!.Value), r.Total, r.ByAccount))
            .ToList();

        return Result<GetDailyBalanceResult>.Success(new GetDailyBalanceResult(result));
    }
}
