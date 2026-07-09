using Clovance.ApiService.Domain.Transactions;

namespace Clovance.ApiService.Features.Transactions.CreateTransaction;

public sealed record CreateTransactionCommand(
    DateOnly Date,
    string Description,
    decimal Amount,
    TransactionType Type,
    Guid AccountId);

public sealed record CreateTransactionResult(
    TransactionDto Transaction);
