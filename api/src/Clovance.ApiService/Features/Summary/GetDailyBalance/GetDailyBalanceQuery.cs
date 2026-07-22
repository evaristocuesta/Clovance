using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetDailyBalance;

public sealed record GetDailyBalanceQuery(Guid? AccountId, int? Year, int? Month);

public sealed record GetDailyBalanceResult(List<DailyBalancePoint> DailyBalance);

public sealed record DailyBalancePoint(
    DateOnly Date,
    decimal Balance,
    List<AccountBalancePoint>? ByAccount);
