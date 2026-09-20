using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Auth.Jwt;
using Clovance.ApiService.Infrastructure.Auth.Refresh;
using Clovance.ApiService.Infrastructure.Auth.Token;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IHandler<LogoutCommand, Result>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClovanceDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        ClovanceDbContext dbContext,
        ITokenService tokenService)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<Result> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var refreshToken = httpContext.Request.GetRefreshTokenCookie();

        if (refreshToken is not null)
        {
            var token = await _dbContext
                .RefreshTokens
                .FirstOrDefaultAsync(t =>
                    t.TokenHash.Equals(RefreshTokenTokenHash.Create(_tokenService.HashToken(refreshToken))),
                    cancellationToken);

            if (token is not null)
            {
                _dbContext.RefreshTokens.Remove(token);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            httpContext.Response.DeleteRefreshTokenCookie();
        }

        return Result.Success();
    }
}
