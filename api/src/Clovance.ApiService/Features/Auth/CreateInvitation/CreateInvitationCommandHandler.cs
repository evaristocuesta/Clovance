using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationCommandHandler : IHandler<CreateInvitationCommand, Result<CreateInvitationResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IOptions<InvitationOptions> _invitationOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateInvitationCommandHandler(
        UserManager<ApplicationUser> userManager,
        ClovanceDbContext dbContext,
        IInvitationTokenService tokenService,
        IOptions<InvitationOptions> invitationOptions,
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
        var httpContext = _httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var adminUserId = _userManager.GetUserId(httpContext.User);
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var normalizedEmail = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.UserAlreadyExists());
        }

        var activeInvitation = _dbContext.UserInvitations
            .FirstOrDefault(i => i.Email == normalizedEmail && i.ConsumedAt == null && i.ExpiresAt > DateTimeOffset.UtcNow);

        if (activeInvitation is not null)
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.ActiveInvitationAlreadyExists());
        }

        var rawToken = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(rawToken);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(Math.Max(1, _invitationOptions.Value.ExpirationHours));

        var invitation = new UserInvitation
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            IsAdmin = request.IsAdmin,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = adminUserId
        };

        _dbContext.UserInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CreateInvitationResult>.Success(new CreateInvitationResult(invitation.Id, invitation.Email, invitation.ExpiresAt, rawToken));
    }
}
