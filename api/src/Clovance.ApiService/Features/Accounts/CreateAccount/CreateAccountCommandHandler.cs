using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;

namespace Clovance.ApiService.Features.Accounts.CreateAccount;

public class CreateAccountCommandHandler : IHandler<CreateAccountCommand, Result<CreateAccountResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public CreateAccountCommandHandler(ClovanceDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<CreateAccountResult>> HandleAsync(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        var email = _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "system";

        var account = await _context.Accounts.AddAsync(
            Account.Create(command.Name, command.Currency, email), 
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<CreateAccountResult>.Success(new CreateAccountResult(account.Entity.ToDto()));
    }
}
