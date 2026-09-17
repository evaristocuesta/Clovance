using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler : IHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager, 
        ClovanceDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<Result> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user is null)
        {
            // Mismo mensaje que un token inválido, para no filtrar si el email existe
            return Result.Failure(AppErrors.Auth.PasswordResetInvalidOrExpiredResetToken());
        }

        var result = await _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword);

        if (!result.Succeeded)
        {
            var isInvalidToken = result.Errors.Any(e => e.Code == "InvalidToken");

            return isInvalidToken
                ? Result.Failure(AppErrors.Auth.PasswordResetInvalidOrExpiredResetToken())
                : Result.Failure(AppErrors.Auth.PasswordResetFailed(
                    string.Join(", ", result.Errors.Select(e => e.Description))));
        }

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == RefreshTokenUserId.Create(user.Id) && !t.IsUsed)
            .ToListAsync(cancellationToken);

        _dbContext.RefreshTokens.RemoveRange(activeRefreshTokens);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
