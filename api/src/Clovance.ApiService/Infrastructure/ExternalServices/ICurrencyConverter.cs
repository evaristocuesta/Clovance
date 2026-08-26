namespace Clovance.ApiService.Infrastructure.ExternalServices;

public interface ICurrencyConverter
{
    Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency, CancellationToken cancellationToken = default);
}
