using Clovance.ApiService.Domain.Transactions;

namespace Clovance.ApiService.Features.Transactions;

public sealed record TransactionDto(
    Guid Id,
    DateOnly Date,
    string Description,
    decimal Amount,
    decimal? PrincipalAmount,
    TransactionType Type,
    Guid AccountId, 
    string AccountName,
    string Currency,
    Guid? RelatedTransactionId);
