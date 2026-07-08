using System.Security.Claims;
using Clovance.ApiService.Domain.UserInvitations;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.UserInvitations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationCommandHandler : IHandler<CreateInvitationCommand, Result<CreateInvitationResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IJwtTokenService _tokenService;
    private readonly IOptions<UserInvitationOptions> _invitationOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateInvitationCommandHandler(
        UserManager<ApplicationUser> userManager,
        ClovanceDbContext dbContext,
        IJwtTokenService tokenService,
        IOptions<UserInvitationOptions> invitationOptions,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _invitationOptions = invitationOptions;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<CreateInvitationResult>> HandleAsync(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.TryParse(
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var email = UserInvitationEmail.Create(request.Email);

        var existingUser = await _userManager.FindByEmailAsync(email.Value);

        if (existingUser is not null)
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.UserAlreadyExists());
        }

        var activeInvitation = await _dbContext
            .UserInvitations
            .FirstOrDefaultAsync(i => i.Email == email && i.ConsumedAt == null && i.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);

        if (activeInvitation is not null)
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.ActiveInvitationAlreadyExists());
        }

        var rawToken = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(rawToken);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(Math.Max(1, _invitationOptions.Value.ExpirationHours));

        var invitation = UserInvitation.Create(email.Value, request.IsAdmin, tokenHash, expiresAt, userId);

        await _dbContext.UserInvitations.AddAsync(invitation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CreateInvitationResult>.Success(new CreateInvitationResult(invitation.Id.Value, invitation.Email.Value, invitation.ExpiresAt, rawToken));
    }
}
