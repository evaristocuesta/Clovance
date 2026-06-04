using Clovance.ApiService.Domain.UserInvitations;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed class RegisterWithInvitationCommandHandler : IHandler<RegisterWithInvitationCommand, Result<RegisterWithInvitationResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IJwtTokenService _tokenService;

    public RegisterWithInvitationCommandHandler(
        UserManager<ApplicationUser> userManager,
        ClovanceDbContext dbContext,
        IJwtTokenService tokenService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<Result<RegisterWithInvitationResult>> HandleAsync(RegisterWithInvitationCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.Token.Trim());

        var invitation = _dbContext.UserInvitations
            .FirstOrDefault(i => i.Email == UserInvitationEmail.Create(request.Email) && i.TokenHash == UserInvitationToken.Create(tokenHash));

        if (invitation is null || invitation.ConsumedAt is not null || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Result<RegisterWithInvitationResult>.Failure(AppErrors.Auth.InvitationInvalidOrExpired());
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return Result<RegisterWithInvitationResult>.Failure(AppErrors.Auth.UserAlreadyExists());
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            MustCompleteOnboarding = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<RegisterWithInvitationResult>.Failure(AppErrors.Auth.UserCreationFailed(errors));
        }

        invitation.Consume(user.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (invitation.IsAdmin)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result<RegisterWithInvitationResult>.Failure(AppErrors.Auth.UserCreationFailed(errors));
            }
        }

        return Result<RegisterWithInvitationResult>.Success(new RegisterWithInvitationResult(user.Id, user.Email));
    }
}
