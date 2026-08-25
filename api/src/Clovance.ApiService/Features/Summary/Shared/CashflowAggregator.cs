namespace Clovance.ApiService.Features.Summary.Shared;

public static class CashflowAggregator
{
    public static List<(PeriodKey Period, decimal Income, decimal Expenses, List<AccountCashflowBreakdown>? ByAccount)> Aggregate(
        IReadOnlyList<PeriodKey> orderedPeriods,
        IReadOnlyDictionary<PeriodKey, List<AccountFlowTotals>> rowsByPeriod,
        bool includeByAccount)
    {
        var result = new List<(PeriodKey, decimal, decimal, List<AccountCashflowBreakdown>?)>();

        foreach (var period in orderedPeriods)
        {
            var rows = rowsByPeriod.GetValueOrDefault(period) ?? [];

            decimal totalIncome;
            decimal totalExpenses;

            if (includeByAccount)
            {
                totalIncome = rows.Sum(r => r.Income);
                totalExpenses = rows.Sum(r => r.Expenses);
            }
            else
            {
                totalIncome = rows.Sum(r => r.AccountIncome);
                totalExpenses = rows.Sum(r => r.AccountExpenses);
            }

            List<AccountCashflowBreakdown>? byAccount = includeByAccount
                ? rows.Select(r => new AccountCashflowBreakdown(r.AccountId.Value, r.AccountIncome, r.AccountExpenses)).ToList()
                : null;

            result.Add((period, totalIncome, totalExpenses, byAccount));
        }

        return result;
    }
}
