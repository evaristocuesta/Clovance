using Clovance.ApiService.Features.Summary.Shared;

namespace Clovance.ApiService.Features.Summary.GetMonthlyCashflow;

public sealed record GetMonthlyCashflowQuery(Guid? AccountId, int MonthsBack = 12, int? AnchorYear = null, int? AnchorMonth = null);

public sealed record GetMonthlyCashflowResult(List<MonthlyCashflowPoint> Points);

public sealed record MonthlyCashflowPoint(
    int Year,
    int Month,
    decimal Income,
    decimal Expenses,
    List<AccountCashflowBreakdown>? ByAccount);
