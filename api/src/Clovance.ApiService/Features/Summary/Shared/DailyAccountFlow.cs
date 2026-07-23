using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Summary.Shared;

public sealed record DailyAccountFlow(
    DateOnly Date,
    AccountId AccountId,
    decimal Income,
    decimal Expenses,
    decimal TransferIn,
    decimal TransferOut)
{
    // Net real of the account that day, including transfers — still used for balance
    public decimal Net => Income + Expenses + TransferIn + TransferOut;
}
