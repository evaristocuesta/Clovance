using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetDailyCashflow;

public sealed record GetDailyCashflowQuery(Guid? AccountId, int? Year, int? Month);

public sealed record GetDailyCashflowResult(List<DailyCashflowPoint> DailyClashFlow);

public sealed record DailyCashflowPoint(
    DateOnly Date,
    decimal Income,
    decimal Expenses,
    List<AccountCashflowBreakdown>? ByAccount);
