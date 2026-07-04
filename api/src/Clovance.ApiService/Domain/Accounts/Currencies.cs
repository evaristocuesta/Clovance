using System.Globalization;

namespace Clovance.ApiService.Domain.Accounts;

public sealed record CurrencyInfo(
    string Code,
    string Symbol,
    string EnglishName);


public class Currencies
{
    public static readonly IReadOnlyDictionary<string, CurrencyInfo> Values =
    CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Select(c => new RegionInfo(c.Name))
        .GroupBy(r => r.ISOCurrencySymbol, StringComparer.OrdinalIgnoreCase)
        .Select(g =>
        {
            var r = g.First();

            return new CurrencyInfo(
                r.ISOCurrencySymbol,
                r.CurrencySymbol,
                r.CurrencyEnglishName);
        })
        .ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);
}
