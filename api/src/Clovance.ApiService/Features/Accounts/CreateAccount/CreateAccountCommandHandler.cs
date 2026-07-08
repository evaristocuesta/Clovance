using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

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
        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ? 
                parsedUserId : 
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<CreateAccountResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var account = await _context.Accounts.AddAsync(
            Account.Create(command.Name, command.Currency, userId), 
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<CreateAccountResult>.Success(new CreateAccountResult(account.Entity.ToDto()));
    }
}
