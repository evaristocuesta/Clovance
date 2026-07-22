using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetMonthlyBalance;

public sealed record GetMonthlyBalanceQuery(Guid? AccountId, int MonthsBack = 12, int? AnchorYear = null, int? AnchorMonth = null);

public sealed record GetMonthlyBalanceResult(List<MonthlyBalancePoint> Points);

public sealed record MonthlyBalancePoint(
    int Year,
    int Month,
    decimal Balance,
    List<AccountBalancePoint>? ByAccount);
