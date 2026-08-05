using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Transactions.UpdateLoanPayment;

public class UpdateLoanPaymentCommandHandler : IHandler<UpdateLoanPaymentCommand, Result<UpdateLoanPaymentResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public UpdateLoanPaymentCommandHandler(ClovanceDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<UpdateLoanPaymentResult>> HandleAsync(UpdateLoanPaymentCommand command, CancellationToken cancellationToken)
    {
        var transaction = await _context
            .Transactions
            .FindAsync(TransactionId.Create(command.TransactionId), cancellationToken);

        if (transaction is null || !transaction.RelatedTransactionId.HasValue)
        {
            return Result<UpdateLoanPaymentResult>.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        var relatedTransaction = await _context
            .Transactions
            .FindAsync(TransactionId.Create(transaction.RelatedTransactionId.Value.Value), cancellationToken);

        if (relatedTransaction is null)
        {
            return Result<UpdateLoanPaymentResult>.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<UpdateLoanPaymentResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var accountFrom = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == AccountId.Create(command.FromAccountId), cancellationToken);

        if (accountFrom is null)
        {
            return Result<UpdateLoanPaymentResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        if (accountFrom.CanBeUsedForFromLoanPayment is false)
        {
            return Result<UpdateLoanPaymentResult>.Failure(AppErrors.Accounts.InvalidAccount());
        }

        var accountTo = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == AccountId.Create(command.ToAccountId), cancellationToken);

        if (accountTo is null)
        {
            return Result<UpdateLoanPaymentResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        if (accountTo.CanBeUsedForToLoanPayment is false)
        {
            return Result<UpdateLoanPaymentResult>.Failure(AppErrors.Accounts.InvalidAccount());
        }

        var fromTransaction = transaction.Amount.Value < 0 ? transaction : relatedTransaction;

        fromTransaction.ChangeDate(TransactionDate.Create(command.Date), userId);
        fromTransaction.ChangeAmount(TransactionAmount.Create(Math.Abs(command.Amount) * -1), userId);
        fromTransaction.ChangePrincipalAmount(TransactionPrincipalAmount.Create(Math.Abs(command.PrincipalAmount) * -1), userId);
        fromTransaction.ChangeDescription(TransactionDescription.Create(command.Description), userId);
        fromTransaction.MoveToAccount(AccountId.Create(command.FromAccountId), userId);

        _context.Transactions.Update(fromTransaction);

        var toTransaction = transaction.Amount.Value < 0 ? relatedTransaction : transaction;

        toTransaction.ChangeDate(TransactionDate.Create(command.Date), userId);
        toTransaction.ChangeAmount(TransactionAmount.Create(Math.Abs(command.PrincipalAmount)), userId);
        toTransaction.ChangeDescription(TransactionDescription.Create(command.Description), userId);
        toTransaction.MoveToAccount(AccountId.Create(command.ToAccountId), userId);

        _context.Transactions.Update(toTransaction);

        _context.SaveChanges();

        return Result<UpdateLoanPaymentResult>.Success(new UpdateLoanPaymentResult
        (
            FromTransaction: fromTransaction.ToDto(),
            ToTransaction: toTransaction.ToDto()
        ));
    }
}
