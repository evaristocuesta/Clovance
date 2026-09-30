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

        if (accountFrom.CanBeUsedForTransfer is false)
        {
            return Result<CreateTransferResult>.Failure(AppErrors.Accounts.InvalidAccount());
        }

        var accountTo = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == AccountId.Create(command.ToAccountId), cancellationToken);

        if (accountTo is null)
        {
            return Result<CreateTransferResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        if (accountTo.CanBeUsedForTransfer is false)
        {
            return Result<CreateTransferResult>.Failure(AppErrors.Accounts.InvalidAccount());
        }

        var transfer = Transaction.CreateTransfer(
            command.Amount,
            command.Description,
            command.FromAccountId,
            command.ToAccountId,
            command.Date,
            userId
        );

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await _context.Transactions.AddRangeAsync([transfer.From, transfer.To], cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        transfer.From.ChangeRelatedTransactionId(transfer.To.Id);
        transfer.To.ChangeRelatedTransactionId(transfer.From.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result<CreateTransferResult>.Success(
            new CreateTransferResult(
                transfer.From.ToDto(), 
                transfer.To.ToDto()));
    }
}
