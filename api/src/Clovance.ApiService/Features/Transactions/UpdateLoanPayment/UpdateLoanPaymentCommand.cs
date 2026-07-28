namespace Clovance.ApiService.Features.Transactions.UpdateLoanPayment;

public sealed record UpdateLoanPaymentRequest(
    DateOnly Date,
    string Description,
    decimal Amount,
    decimal PrincipalAmount,
    Guid FromAccountId,
    Guid ToAccountId);

public sealed record UpdateLoanPaymentCommand(
    Guid TransactionId,
    DateOnly Date,
    string Description,
    decimal Amount,
    decimal PrincipalAmount,
    Guid FromAccountId,
    Guid ToAccountId);

public sealed record UpdateLoanPaymentResult(
    TransactionDto FromTransaction,
    TransactionDto ToTransaction);
