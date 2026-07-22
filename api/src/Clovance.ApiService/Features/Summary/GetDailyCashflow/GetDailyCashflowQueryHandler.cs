using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Clovance.ApiService.Features.Summary.GetDailyCashflow;

public class GetDailyCashflowQueryHandler : IHandler<GetDailyCashflowQuery, Result<GetDailyCashflowResult>>
{
    private readonly ClovanceDbContext _context;

    public GetDailyCashflowQueryHandler(ClovanceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetDailyCashflowResult>> HandleAsync(GetDailyCashflowQuery query, CancellationToken cancellationToken)
    {
        var monthStart = new DateOnly(query.Year ?? DateTime.UtcNow.Year, query.Month ?? DateTime.UtcNow.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var baseQuery = _context.Transactions
            .Where(t => query.AccountId == null || t.AccountId == AccountId.Create(query.AccountId.Value));

        var dailyFlows = await TransactionSummaryQueries.GetDailyFlowsAsync(baseQuery, monthStart, monthEnd, cancellationToken);

        var rowsByPeriod = dailyFlows
            .GroupBy(f => PeriodKey.Daily(f.Date))
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => (f.AccountId, f.Income, f.Expenses)).ToList());

        var orderedPeriods = Enumerable.Range(0, monthEnd.DayNumber - monthStart.DayNumber + 1)
            .Select(i => PeriodKey.Daily(monthStart.AddDays(i)))
            .ToList();

        var aggregated = CashflowAggregator.Aggregate(
            orderedPeriods,
            rowsByPeriod,
            includeByAccount: query.AccountId is null);

        var result = aggregated
            .Select(a => new DailyCashflowPoint(new DateOnly(a.Period.Year, a.Period.Month, a.Period.Day!.Value), a.Income, a.Expenses, a.ByAccount))
            .ToList();

        return Result<GetDailyCashflowResult>.Success(new GetDailyCashflowResult(result));
    }
}
