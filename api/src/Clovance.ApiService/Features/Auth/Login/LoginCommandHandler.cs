using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginCommandHandler : IHandler<LoginCommand, Result<LoginResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResult>> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
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

        return Result<LoginResult>.Success(new LoginResult(token, expiresAt));
    }
}
