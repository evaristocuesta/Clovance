using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Accounts.CreateAccount;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;

namespace Clovance.ApiService.Features.Transactions.CreateTransaction;

public class CreateTransactionCommandHandler : IHandler<CreateTransactionCommand, Result<CreateTransactionResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public CreateTransactionCommandHandler(ClovanceDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<CreateTransactionResult>> HandleAsync(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<CreateTransactionResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var account = await _context.Accounts.FindAsync(new object[] { command.AccountId }, cancellationToken);

        if (account is null)
        {
            return Result<CreateTransactionResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var transaction = await _context.Transactions.AddAsync(
            Transaction.Create(
                command.Amount,
                command.Description,
                command.AccountId,
                command.Date,
                userId
            )
        );
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CreateTransactionResult>.Success(new CreateTransactionResult(transaction.Entity.ToDto()));
    }
}
