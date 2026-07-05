using System.Globalization;

namespace Clovance.ApiService.Domain.Accounts;

public sealed record CurrencyInfo(
    string Code,
    string Symbol,
    string Name);


public class Currencies
{
    public static readonly IReadOnlyDictionary<string, CurrencyInfo> Values =
    CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Select(c => new RegionInfo(c.Name))
        .Where(r =>
            r.ISOCurrencySymbol.Length == 3 &&
            r.ISOCurrencySymbol.All(char.IsLetter) &&
            !string.IsNullOrWhiteSpace(r.CurrencyEnglishName))
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
