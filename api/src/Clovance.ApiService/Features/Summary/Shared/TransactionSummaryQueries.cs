using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Summary.Shared;

public static class TransactionSummaryQueries
{
    public static async Task<List<DailyAccountFlow>> GetDailyFlowsAsync(
        IQueryable<Transaction> baseQuery,
        DateOnly from,
        DateOnly to,
        bool excludeLiabilityAccounts,
        string targetCurrency,
        ICurrencyConverter currencyConverter,
        CancellationToken ct)
    {
        var fromDate = TransactionDate.Create(from);
        var toDate = TransactionDate.Create(to);

        var query = baseQuery
            .Where(t => t.Date >= fromDate && t.Date <= toDate);

        if (excludeLiabilityAccounts)
        {
            query = query.Where(t => 
                t.Account.Type != AccountType.CreditCard && 
                t.Account.Type != AccountType.Loan && 
                t.Account.Type != AccountType.Mortgage);
        }

        var raw = await query
            .Select(t => new { t.Date, t.AccountId, t.Amount, t.Type, t.Account.Currency })
            .ToListAsync(ct);

        var uniqueCurrencies = raw.Select(t => t.Currency.Code).Distinct().ToList();
        var exchangeRates = await GetExchangeRatesForCurrenciesAsync(uniqueCurrencies, targetCurrency, currencyConverter, ct);

        return raw
            .GroupBy(t => new { t.Date, t.AccountId })
            .Select(g =>
            {
                var accountCurrency = g.First().Currency.Code;
                var rate = exchangeRates[accountCurrency];

                return new DailyAccountFlow(
                    g.Key.Date.Value,
                    g.Key.AccountId,
                    Income: g.Sum(t => t.Type != TransactionType.Transfer && t.Amount.Value > 0 ? t.Amount.Value * rate : 0m),
                    Expenses: g.Sum(t => t.Type != TransactionType.Transfer && t.Amount.Value < 0 ? t.Amount.Value * rate : 0m),
                    TransferIn: g.Sum(t => t.Type == TransactionType.Transfer && t.Amount.Value > 0 ? t.Amount.Value * rate : 0m),
                    TransferOut: g.Sum(t => t.Type == TransactionType.Transfer && t.Amount.Value < 0 ? t.Amount.Value * rate : 0m));
            })
            .ToList();
    }

    public static async Task<Dictionary<AccountId, decimal>> GetOpeningBalancesAsync(
        IQueryable<Transaction> baseQuery,
        DateOnly before,
        bool excludeLiabilityAccounts,
        string targetCurrency,
        ICurrencyConverter currencyConverter,
        CancellationToken ct)
    {
        var beforeDate = TransactionDate.Create(before);

        var query = baseQuery.Where(t => t.Date < beforeDate);

        if (excludeLiabilityAccounts)
        {
            query = query.Where(t => 
                t.Account.Type != AccountType.CreditCard && 
                t.Account.Type != AccountType.Loan && 
                t.Account.Type != AccountType.Mortgage);
        }

        var raw = await query
            .Select(t => new { t.AccountId, t.Amount, t.Account.Currency })
            .ToListAsync(ct);

        var uniqueCurrencies = raw.Select(t => t.Currency.Code).Distinct().ToList();
        var exchangeRates = await GetExchangeRatesForCurrenciesAsync(uniqueCurrencies, targetCurrency, currencyConverter, ct);

        return raw
            .GroupBy(t => t.AccountId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var accountCurrency = g.First().Currency.Code;
                    var rate = exchangeRates[accountCurrency];
                    return g.Sum(t => t.Amount.Value * rate);
                });
    }

    private static async Task<Dictionary<string, decimal>> GetExchangeRatesForCurrenciesAsync(
        List<string> currencies,
        string targetCurrency,
        ICurrencyConverter currencyConverter,
        CancellationToken ct)
    {
        if (currencies.Count == 1 && currencies[0].Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, decimal> { [targetCurrency] = 1.0m };
        }

        var rates = await currencyConverter.GetExchangeRatesAsync(targetCurrency, ct);

        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var currency in currencies)
        {
            if (currency.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
            {
                result[currency] = 1.0m;
            }
            else if (rates.TryGetValue(currency, out var rate))
            {
                result[currency] = 1.0m / rate;
            }
            else
            {
                throw new InvalidOperationException($"Exchange rate not available for currency: {currency}");
            }
        }

        return result;
    }
}
