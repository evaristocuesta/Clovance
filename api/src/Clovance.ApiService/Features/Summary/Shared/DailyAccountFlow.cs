using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Summary.Shared;

public sealed record DailyAccountFlow(DateOnly Date, AccountId AccountId, decimal Income, decimal Expenses)
{
    public decimal Net => Income + Expenses;
}
