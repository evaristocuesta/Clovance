namespace Clovance.ApiService.Features.Transactions.GetTransactionById;

public sealed record GetTransactionByIdQuery(Guid Id);

public sealed record GetTransactionByIdResult(TransactionDto? Transaction);
