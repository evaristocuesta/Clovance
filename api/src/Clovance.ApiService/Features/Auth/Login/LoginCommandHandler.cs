using System.Security.Claims;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginCommandHandler
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginCommandHandler(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var tokenResult = await _signInManager.CreateUserPrincipalAsync(user);

        if (tokenResult.Identity is ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim("must_complete_onboarding", user.MustCompleteOnboarding.ToString()));
        }

        return new LoginResult(tokenResult);
    }
}
