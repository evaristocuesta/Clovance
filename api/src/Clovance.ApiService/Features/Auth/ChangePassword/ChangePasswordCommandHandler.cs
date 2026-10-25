using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Auth.Refresh;
using Clovance.ApiService.Infrastructure.Auth.Token;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IHandler<ChangePasswordCommand, Result>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;
    private readonly ClovanceDbContext _dbContext;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService,
        ClovanceDbContext dbContext)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<Result> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var user = await _userManager.GetUserAsync(httpContext.User);

        if (user is null)
        {
            return Result<Result>.Failure(AppErrors.Auth.UserNotFound());
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        if (!changePasswordResult.Succeeded)
        {
            var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            return Result<Result>.Failure(AppErrors.Auth.PasswordChangeFailed(errors));
        }

        // Revoke every active session (including any stolen one) and give the current
        // session a fresh refresh token so the user is not logged out.
        var userId = RefreshTokenUserId.Create(user.Id);

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync(cancellationToken);

        _dbContext.RefreshTokens.RemoveRange(activeRefreshTokens);

        var newRefreshToken = _tokenService.GenerateToken();
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.Add(RefreshTokenLifetime);

        await _dbContext.RefreshTokens.AddAsync(
            RefreshToken.Create(user.Id, _tokenService.HashToken(newRefreshToken), refreshTokenExpiresAt),
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        httpContext.Response.SetRefreshTokenCookie(newRefreshToken, refreshTokenExpiresAt);

        return Result.Success();
    }
}
