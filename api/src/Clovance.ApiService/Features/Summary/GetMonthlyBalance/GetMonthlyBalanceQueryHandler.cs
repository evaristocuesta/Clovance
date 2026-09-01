using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Summary.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Summary.GetMonthlyBalance;

public sealed class GetMonthlyBalanceQueryHandler : IHandler<GetMonthlyBalanceQuery, Result<GetMonthlyBalanceResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly ICurrencyConverter _currencyConverter;

    public GetMonthlyBalanceQueryHandler(ClovanceDbContext context, ICurrencyConverter currencyConverter)
    {
        _context = context;
        _currencyConverter = currencyConverter;
    }

    public async Task<Result<GetMonthlyBalanceResult>> HandleAsync(GetMonthlyBalanceQuery query, CancellationToken cancellationToken)
    {
        var anchor = new DateOnly(
            query.AnchorYear ?? DateTime.UtcNow.Year,
            query.AnchorMonth ?? DateTime.UtcNow.Month,
            1);

        var windowStart = anchor.AddMonths(-(query.MonthsBack - 1));
        var windowEnd = anchor.AddMonths(1).AddDays(-1);

        var baseQuery = _context.Transactions
            .AsNoTracking()
            .Where(t => query.AccountId == null || t.AccountId == AccountId.Create(query.AccountId.Value));

        var openingByAccount = await TransactionSummaryQueries.GetOpeningBalancesAsync(baseQuery, windowStart, query.AccountType, query.Currency, _currencyConverter, cancellationToken);
        var dailyFlows = await TransactionSummaryQueries.GetDailyFlowsAsync(baseQuery, windowStart, windowEnd, query.AccountType, query.Currency, _currencyConverter, cancellationToken);

        var netsByPeriod = dailyFlows
            .GroupBy(f => PeriodKey.Monthly(f.Date.Year, f.Date.Month))
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(f => f.AccountId)
                      .Select(a => (a.Key, a.Sum(f => f.Net)))
                      .ToList());

        var orderedPeriods = Enumerable.Range(0, query.MonthsBack)
            .Select(i => PeriodKey.Monthly(windowStart.AddMonths(i).Year, windowStart.AddMonths(i).Month))
            .ToList();

        var rolled = BalanceRollForward.Calculate(
            orderedPeriods,
            openingByAccount,
            netsByPeriod,
            includeByAccount: query.AccountId is null);

        var result = rolled
            .Select(r => new MonthlyBalancePoint(r.Period.Year, r.Period.Month, r.Total, r.ByAccount))
            .ToList();

        return Result<GetMonthlyBalanceResult>.Success(new GetMonthlyBalanceResult(result));
    }
}
