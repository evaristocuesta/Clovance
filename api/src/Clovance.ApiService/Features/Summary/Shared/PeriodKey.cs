namespace Clovance.ApiService.Features.Summary.Shared;

public readonly record struct PeriodKey(int Year, int Month, int? Day)
{
    public static PeriodKey Monthly(int year, int month) => new(year, month, null);
    public static PeriodKey Daily(DateOnly date) => new(date.Year, date.Month, date.Day);
}
