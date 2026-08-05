using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Transactions.CreateLoanPayment;

public class CreateLoanPaymentCommandHandler : IHandler<CreateLoanPaymentCommand, Result<CreateLoanPaymentResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public CreateLoanPaymentCommandHandler(ClovanceDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<CreateLoanPaymentResult>> HandleAsync(CreateLoanPaymentCommand command, CancellationToken cancellationToken)
    {
        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<CreateLoanPaymentResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var accountFrom = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == AccountId.Create(command.FromAccountId), cancellationToken);

        if (accountFrom is null)
        {
            return Result<CreateLoanPaymentResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        if (accountFrom.CanBeUsedForFromLoanPayment is false)
        {
            return Result<CreateLoanPaymentResult>.Failure(AppErrors.Accounts.InvalidAccount());
        }

        var accountTo = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == AccountId.Create(command.ToAccountId), cancellationToken);

        if (accountTo is null)
        {
            return Result<CreateLoanPaymentResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        if (accountTo.CanBeUsedForToLoanPayment is false)
        {
            return Result<CreateLoanPaymentResult>.Failure(AppErrors.Accounts.InvalidAccount());
        }

        var loanPayment = Transaction.CreateLoanPayment(
            command.Amount,
            command.PrincipalAmount,
            command.Description,
            command.FromAccountId,
            command.ToAccountId,
            command.Date,
            userId
        );

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await _context.Transactions.AddRangeAsync([loanPayment.From, loanPayment.To], cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        loanPayment.From.ChangeRelatedTransactionId(loanPayment.To.Id);
        loanPayment.To.ChangeRelatedTransactionId(loanPayment.From.Id);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<CreateLoanPaymentResult>.Success(
            new CreateLoanPaymentResult(
                loanPayment.From.ToDto(),
                loanPayment.To.ToDto()));
    }
}
