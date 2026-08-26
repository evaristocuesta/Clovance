using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Clovance.ApiService.Infrastructure.ExternalServices;

public sealed class FrankfurterCurrencyConverter : ICurrencyConverter
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FrankfurterCurrencyConverter> _logger;
    private const string BaseUrl = "https://api.frankfurter.app";

    public FrankfurterCurrencyConverter(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<FrankfurterCurrencyConverter> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"exchange_rates_{baseCurrency.ToUpperInvariant()}";

        if (_cache.TryGetValue<Dictionary<string, decimal>>(cacheKey, out var cachedRates))
        {
            _logger.LogDebug("Using cached exchange rates for {BaseCurrency}", baseCurrency);
            return cachedRates!;
        }

        try
        {
            _logger.LogInformation("Fetching exchange rates from Frankfurter API for {BaseCurrency}", baseCurrency);

            var response = await _httpClient.GetAsync($"{BaseUrl}/latest?from={baseCurrency.ToUpperInvariant()}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<FrankfurterResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Rates == null)
            {
                throw new InvalidOperationException("Failed to parse exchange rates from Frankfurter API");
            }

            var rates = result.Rates;
            rates[baseCurrency.ToUpperInvariant()] = 1.0m;

            _cache.Set(cacheKey, rates, TimeSpan.FromHours(24));

            _logger.LogInformation("Successfully cached exchange rates for {BaseCurrency} (24h)", baseCurrency);

            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rates from Frankfurter API for {BaseCurrency}", baseCurrency);
            throw;
        }
    }

    private sealed record FrankfurterResponse(
        decimal Amount,
        string Base,
        string Date,
        Dictionary<string, decimal> Rates);
}
