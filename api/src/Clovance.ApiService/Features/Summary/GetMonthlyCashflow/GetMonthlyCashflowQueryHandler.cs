using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Summary.GetMonthlyCashflow;

public class GetMonthlyCashflowQueryHandler : IHandler<GetMonthlyCashflowQuery, Result<GetMonthlyCashflowResult>>
{
    private readonly ClovanceDbContext _context;

    public GetMonthlyCashflowQueryHandler(ClovanceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetMonthlyCashflowResult>> HandleAsync(GetMonthlyCashflowQuery query, CancellationToken cancellationToken)
    {
        var anchor = new DateOnly(
            query.AnchorYear ?? DateTime.UtcNow.Year,
            query.AnchorMonth ?? DateTime.UtcNow.Month,
            1);

        var windowStart = anchor.AddMonths(-(query.MonthsBack - 1));
        var windowEnd = anchor.AddMonths(1).AddDays(-1);

        var baseQuery = _context.Transactions
            .Where(t => query.AccountId == null || t.AccountId == AccountId.Create(query.AccountId.Value));

        var dailyFlows = await TransactionSummaryQueries.GetDailyFlowsAsync(baseQuery, windowStart, windowEnd, cancellationToken);

        var rowsByPeriod = dailyFlows
            .GroupBy(f => PeriodKey.Monthly(f.Date.Year, f.Date.Month))
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(f => f.AccountId)
                      .Select(a => new AccountFlowTotals(
                          a.Key,
                          a.Sum(f => f.Income),
                          a.Sum(f => f.Expenses),
                          a.Sum(f => f.TransferIn),
                          a.Sum(f => f.TransferOut)))
                      .ToList());

        var orderedPeriods = Enumerable.Range(0, query.MonthsBack)
            .Select(i => PeriodKey.Monthly(windowStart.AddMonths(i).Year, windowStart.AddMonths(i).Month))
            .ToList();

        var aggregated = CashflowAggregator.Aggregate(orderedPeriods, rowsByPeriod, includeByAccount: query.AccountId is null);

        var result = aggregated
            .Select(a => new MonthlyCashflowPoint(a.Period.Year, a.Period.Month, a.Income, a.Expenses, a.ByAccount))
            .ToList();

        return Result<GetMonthlyCashflowResult>.Success(new GetMonthlyCashflowResult(result));
    }
}
