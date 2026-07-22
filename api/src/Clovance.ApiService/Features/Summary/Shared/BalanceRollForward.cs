using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Summary.Shared;

public static class BalanceRollForward
{
    public static List<(PeriodKey Period, decimal Total, List<AccountBalancePoint>? ByAccount)> Calculate(
        IReadOnlyList<PeriodKey> orderedPeriods,
        IReadOnlyDictionary<AccountId, decimal> openingByAccount,
        IReadOnlyDictionary<PeriodKey, List<(AccountId AccountId, decimal Net)>> netsByPeriod,
        bool includeByAccount)
    {
        var accountIds = openingByAccount.Keys
            .Union(netsByPeriod.Values.SelectMany(rows => rows.Select(r => r.AccountId)))
            .Distinct()
            .ToList();

        var running = accountIds.ToDictionary(id => id, id => openingByAccount.GetValueOrDefault(id));
        var result = new List<(PeriodKey, decimal, List<AccountBalancePoint>?)>();

        foreach (var period in orderedPeriods)
        {
            var netsThisPeriod = (netsByPeriod.GetValueOrDefault(period) ?? [])
                .ToDictionary(r => r.AccountId, r => r.Net);

            foreach (var id in accountIds)
            {
                running[id] += netsThisPeriod.GetValueOrDefault(id);
            }

            var byAccount = includeByAccount
                ? accountIds.Select(id => new AccountBalancePoint(id.Value, running[id])).ToList()
                : null;

            result.Add((period, running.Values.Sum(), byAccount));
        }

        return result;
    }
}
