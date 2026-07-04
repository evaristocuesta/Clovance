using System.Globalization;

namespace Clovance.ApiService.Domain.Accounts;

public static class CurrencyValidator
{
    public static bool IsValid(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return false;

        return Currencies.Values.Keys.Contains(currency, StringComparer.OrdinalIgnoreCase);
    }
}
