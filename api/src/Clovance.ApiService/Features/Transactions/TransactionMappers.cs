

using Clovance.ApiService.Domain.Transactions;

namespace Clovance.ApiService.Features.Transactions;

public static class TransactionMappers
{
    public static TransactionDto ToDto(
        this Transaction transaction)
    {
        return new(
            transaction.Id.Value,
            transaction.Date.Value,
            transaction.Description.Value,
            transaction.Amount.Value,
            transaction.AccountId.Value);
    }
}
