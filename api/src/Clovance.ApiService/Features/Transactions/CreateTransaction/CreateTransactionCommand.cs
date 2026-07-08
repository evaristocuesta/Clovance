namespace Clovance.ApiService.Features.Transactions.CreateTransaction;

public sealed record CreateTransactionCommand(
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid AccountId);

public sealed record CreateTransactionResult(
    TransactionDto Transaction);
