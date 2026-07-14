using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Transactions.CreateTranfer;

public class CreateTransferCommandHandler : IHandler<CreateTransferCommand, Result<CreateTransferResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public CreateTransferCommandHandler(ClovanceDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<CreateTransferResult>> HandleAsync(CreateTransferCommand command, CancellationToken cancellationToken)
    {
        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<CreateTransferResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var accountFrom = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == AccountId.Create(command.FromAccountId), cancellationToken);

        if (accountFrom is null)
        {
            return Result<CreateTransferResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var accountTo = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == AccountId.Create(command.ToAccountId), cancellationToken);

        if (accountTo is null)
        {
            return Result<CreateTransferResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var from = Transaction.Create(
                command.Amount * -1,
                TransactionType.Transfer,
                command.Description,
                command.FromAccountId,
                command.Date,
                userId
            );

        var to = Transaction.Create(
            command.Amount,
            TransactionType.Transfer,
            command.Description,
            command.ToAccountId,
            command.Date,
            userId
        );

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await _context.Transactions.AddRangeAsync([from, to], cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        from.ChangeRelatedTransactionId(to.Id);
        to.ChangeRelatedTransactionId(from.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result<CreateTransferResult>.Success(
            new CreateTransferResult(
                from.ToDto(), 
                to.ToDto()));
    }
}
