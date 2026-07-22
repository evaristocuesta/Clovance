using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Summary.Shared;

public static class CashflowAggregator
{
    public static List<(PeriodKey Period, decimal Income, decimal Expenses, List<AccountCashflowBreakdown>? ByAccount)> Aggregate(
        IReadOnlyList<PeriodKey> orderedPeriods,
        IReadOnlyDictionary<PeriodKey, List<(AccountId AccountId, decimal Income, decimal Expenses)>> rowsByPeriod,
        bool includeByAccount)
    {
        var result = new List<(PeriodKey, decimal, decimal, List<AccountCashflowBreakdown>?)>();

        foreach (var period in orderedPeriods)
        {
            var rows = rowsByPeriod.GetValueOrDefault(period) ?? [];

            var byAccount = includeByAccount
                ? rows.Select(r => new AccountCashflowBreakdown(r.AccountId.Value, r.Income, r.Expenses)).ToList()
                : null;

            result.Add((period, rows.Sum(r => r.Income), rows.Sum(r => r.Expenses), byAccount));
        }

        return result;
    }
}
