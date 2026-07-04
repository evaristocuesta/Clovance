using System.Globalization;

namespace Clovance.ApiService.Domain.Accounts;

public class Currencies
{
    public static readonly HashSet<string> Values =
    CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Select(c => new RegionInfo(c.Name).ISOCurrencySymbol)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
