using System.Globalization;

namespace Clovance.ApiService.Domain.Accounts;

public static class CurrencyValidator
{
    private static readonly HashSet<string> ValidCurrencies =
        CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => new RegionInfo(c.Name).ISOCurrencySymbol)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsValid(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return false;

        return ValidCurrencies.Contains(currency);
    }
}
