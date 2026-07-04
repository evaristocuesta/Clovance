namespace Clovance.ApiService.Features.Accounts.GetCurrencies;

public sealed record GetCurrenciesQuery();

public sealed record GetCurrenciesResult(IEnumerable<string> Currencies);
