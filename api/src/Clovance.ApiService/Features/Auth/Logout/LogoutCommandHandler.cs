using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IHandler<LogoutCommand, Result>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClovanceDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public LogoutCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        ClovanceDbContext dbContext,
        IJwtTokenService jwtTokenService)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var refreshToken = httpContext.Request.Cookies["refreshToken"];

        if (refreshToken is not null)
        {
            var token = await _dbContext
                .RefreshTokens
                .FirstOrDefaultAsync(t =>
                    t.Token.Equals(RefreshTokenToken.Create(_jwtTokenService.HashToken(refreshToken))),
                    cancellationToken);

            if (token is not null)
            {
                _dbContext.RefreshTokens.Remove(token);
                await _dbContext.SaveChangesAsync(cancellationToken);
                httpContext.Response.Cookies.Delete("refreshToken");
            }
        }

        return Result.Success();
    }
}
