using Clovance.ApiService.Exceptions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed class RegisterWithInvitationCommandHandler : IHandler<RegisterWithInvitationCommand, RegisterWithInvitationResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;

    public RegisterWithInvitationCommandHandler(
        UserManager<ApplicationUser> userManager,
        ClovanceDbContext dbContext,
        IInvitationTokenService tokenService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<RegisterWithInvitationResult> HandleAsync(RegisterWithInvitationCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var tokenHash = _tokenService.HashToken(request.Token.Trim());

        var invitation = _dbContext.UserInvitations
            .FirstOrDefault(i => i.Email == normalizedEmail && i.TokenHash == tokenHash);

        if (invitation is null || invitation.ConsumedAt is not null || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedException("Invalid or expired invitation.");
        }

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            MustCompleteOnboarding = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new ConflictException($"Failed to create user: {errors}");
        }

        invitation.ConsumedAt = DateTimeOffset.UtcNow;
        invitation.ConsumedByUserId = user.Id;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterWithInvitationResult(user.Id, user.Email);
    }
}
