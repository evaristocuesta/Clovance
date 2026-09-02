using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Auth.Refresh;

public sealed class RefreshCommandHandler : IHandler<RefreshCommand, Result<RefreshResult>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;

    public RefreshCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        IJwtTokenService jwtTokenService,
        UserManager<ApplicationUser> userManager,
        ClovanceDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtTokenService = jwtTokenService;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<Result<RefreshResult>> HandleAsync(RefreshCommand command, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HttpContext is not available.");

        var refreshTokenValue = httpContext.Request.GetRefreshTokenCookie();

        if (string.IsNullOrEmpty(refreshTokenValue))
            return Result<RefreshResult>.Failure(AppErrors.Auth.UserNotAuthenticated());

        var hashedToken = _jwtTokenService.HashToken(refreshTokenValue);

        var storedToken = await _dbContext
            .RefreshTokens
            .FirstOrDefaultAsync(t =>
                t.Token == RefreshTokenToken.Create(hashedToken) &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (storedToken is null)
            return Result<RefreshResult>.Failure(AppErrors.Auth.UserNotAuthenticated());

        var userId = storedToken.UserId.Value;

        storedToken.MarkAsUsed();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result<RefreshResult>.Failure(AppErrors.Auth.UserNotFound());
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenService.GenerateToken(userId, user.Email!, roles);
        var newRefreshToken = _jwtTokenService.GenerateToken();
        var refreshTokenExpiresAt = newAccessToken.ExpiresAt.AddDays(7);

        await _dbContext
            .RefreshTokens
            .AddAsync(
                RefreshToken.Create(
                    userId,
                    _jwtTokenService.HashToken(newRefreshToken),
                    refreshTokenExpiresAt),
                cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        
        httpContext.Response.SetRefreshTokenCookie(newRefreshToken, refreshTokenExpiresAt);

        return Result<RefreshResult>.Success(new RefreshResult(Token: newAccessToken.Token, ExpiresAt: newAccessToken.ExpiresAt));
    }
}
