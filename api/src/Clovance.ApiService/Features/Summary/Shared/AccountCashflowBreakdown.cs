namespace Clovance.ApiService.Features.Summary.Shared;

public sealed record AccountCashflowBreakdown(Guid AccountId, decimal Income, decimal Expenses);
