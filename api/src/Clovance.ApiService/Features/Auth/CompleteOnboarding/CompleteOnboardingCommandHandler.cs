using Clovance.ApiService.Exceptions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed class CompleteOnboardingCommandHandler : IHandler<CompleteOnboardingCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CompleteOnboardingCommandHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Unit> HandleAsync(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var userId = _userManager.GetUserId(httpContext.User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("User is not authenticated.", ErrorCodes.Auth.UserNotAuthenticated);
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new UnauthorizedException("User not found.", ErrorCodes.Auth.UserNotFound);
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!changeResult.Succeeded)
        {
            var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
            throw new ConflictException($"Failed to change password: {errors}", ErrorCodes.Auth.PasswordChangeFailed);
        }

        var normalizedEmail = request.NewEmail.Trim();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser is not null && !string.Equals(existingUser.Id, user.Id, StringComparison.Ordinal))
            {
                throw new ConflictException("Email is already in use.", ErrorCodes.Auth.EmailAlreadyInUse);
            }

            var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, normalizedEmail);
            var emailResult = await _userManager.ChangeEmailAsync(user, normalizedEmail, emailToken);
            if (!emailResult.Succeeded)
            {
                var errors = string.Join(", ", emailResult.Errors.Select(e => e.Description));
                throw new ConflictException($"Failed to change email: {errors}", ErrorCodes.Auth.EmailChangeFailed);
            }

            var usernameResult = await _userManager.SetUserNameAsync(user, normalizedEmail);
            if (!usernameResult.Succeeded)
            {
                var errors = string.Join(", ", usernameResult.Errors.Select(e => e.Description));
                throw new ConflictException($"Failed to change username: {errors}", ErrorCodes.Auth.UsernameChangeFailed);
            }
        }

        user.MustCompleteOnboarding = false;
        await _userManager.UpdateAsync(user);

        return Unit.Value;
    }
}
