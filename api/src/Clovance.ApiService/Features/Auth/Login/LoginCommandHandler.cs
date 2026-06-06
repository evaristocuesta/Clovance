using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginCommandHandler : IHandler<LoginCommand, Result<LoginResult>>
{
    private readonly ClovanceDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        ClovanceDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResult>> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Result<LoginResult>.Failure(AppErrors.Auth.InvalidCredentials());
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            return Result<LoginResult>.Failure(AppErrors.Auth.InvalidCredentials());
        }

        var roles = await _userManager.GetRolesAsync(user);

        var (token, expiresAt) = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email ?? string.Empty,
            roles,
            user.MustCompleteOnboarding);

        var refreshToken = _jwtTokenService.GenerateToken();

        await _dbContext
            .RefreshTokens
            .AddAsync(
                RefreshToken.Create(
                    user.Id,
                    _jwtTokenService.HashToken(refreshToken),
                    expiresAt.AddDays(7))
                , cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);


        httpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt.AddDays(7).UtcDateTime
        });

        return Result<LoginResult>.Success(new LoginResult(token, expiresAt));
    }
}
