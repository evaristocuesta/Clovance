namespace Clovance.ApiService.Features.Transactions.CreateLoanPayment;

public sealed record CreateLoanPaymentCommand(
    DateOnly Date,
    string Description,
    decimal Amount,
    decimal PrincipalAmount,
    Guid FromAccountId,
    Guid ToAccountId);

public sealed record CreateLoanPaymentResult(
    TransactionDto FromTransaction,
    TransactionDto ToTransaction);
