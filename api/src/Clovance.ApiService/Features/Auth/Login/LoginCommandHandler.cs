using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Clovance.ApiService.Exceptions;
using Clovance.ApiService.Shared;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginCommandHandler : IHandler<LoginCommand, LoginResult>
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

    public async Task<LoginResult> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid credentials.", ErrorCodes.Auth.InvalidCredentials);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid credentials.", ErrorCodes.Auth.InvalidCredentials);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email ?? string.Empty,
            roles,
            user.MustCompleteOnboarding);

        return new LoginResult(token, expiresAt);
    }
}
