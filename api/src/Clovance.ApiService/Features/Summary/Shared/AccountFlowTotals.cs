using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Summary.Shared;

public sealed record AccountFlowTotals(AccountId AccountId, decimal Income, decimal Expenses, decimal TransferIn, decimal TransferOut)
{
    // Individual total account: includes transfers
    public decimal AccountIncome => Income + TransferIn;
    public decimal AccountExpenses => Expenses + TransferOut;
}
