using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Summary.Shared;

public static class TransactionSummaryQueries
{
    public static async Task<List<DailyAccountFlow>> GetDailyFlowsAsync(
        IQueryable<Transaction> baseQuery,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var fromDate = TransactionDate.Create(from);
        var toDate = TransactionDate.Create(to);

        var raw = await baseQuery
            .Where(t => t.Date >= fromDate && t.Date <= toDate)
            .Select(t => new { t.Date, t.AccountId, t.Amount, t.Type })
            .ToListAsync(ct);

        return raw
            .GroupBy(t => new { t.Date, t.AccountId })
            .Select(g => new DailyAccountFlow(
                g.Key.Date.Value,
                g.Key.AccountId,
                Income: g.Sum(t => t.Type != TransactionType.Transfer && t.Amount.Value > 0 ? t.Amount.Value : 0m),
                Expenses: g.Sum(t => t.Type != TransactionType.Transfer && t.Amount.Value < 0 ? t.Amount.Value : 0m),
                TransferIn: g.Sum(t => t.Type == TransactionType.Transfer && t.Amount.Value > 0 ? t.Amount.Value : 0m),
                TransferOut: g.Sum(t => t.Type == TransactionType.Transfer && t.Amount.Value < 0 ? t.Amount.Value : 0m)))
            .ToList();
    }

    public static async Task<Dictionary<AccountId, decimal>> GetOpeningBalancesAsync(
        IQueryable<Transaction> baseQuery,
        DateOnly before,
        CancellationToken ct)
    {
        var beforeDate = TransactionDate.Create(before);

        var raw = await baseQuery
            .Where(t => t.Date < beforeDate)
            .Select(t => new { t.AccountId, t.Amount })
            .ToListAsync(ct);

        return raw
            .GroupBy(t => t.AccountId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount.Value));
    }
}
