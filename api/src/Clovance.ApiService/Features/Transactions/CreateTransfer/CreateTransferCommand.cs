using Clovance.ApiService.Domain.Transactions;

namespace Clovance.ApiService.Features.Transactions.CreateTranfer;

public sealed record CreateTransferCommand(
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid FromAccountId,
    Guid ToAccountId);

public sealed record CreateTransferResult(
    TransactionDto FromTransaction,
    TransactionDto ToTransaction);
