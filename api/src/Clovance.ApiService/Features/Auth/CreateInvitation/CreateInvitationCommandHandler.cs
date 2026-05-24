using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationCommandHandler : IHandler<CreateInvitationCommand, CreateInvitationResult>
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

    public async Task<CreateInvitationResult> HandleAsync(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var adminUserId = _userManager.GetUserId(httpContext.User);
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var normalizedEmail = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var activeInvitation = _dbContext.UserInvitations
            .FirstOrDefault(i => i.Email == normalizedEmail && i.ConsumedAt == null && i.ExpiresAt > DateTimeOffset.UtcNow);

        if (activeInvitation is not null)
        {
            throw new InvalidOperationException("There is already an active invitation for this email.");
        }

        var rawToken = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(rawToken);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(Math.Max(1, _invitationOptions.Value.ExpirationHours));

        var invitation = new UserInvitation
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = adminUserId
        };

        _dbContext.UserInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateInvitationResult(invitation.Id, invitation.Email, invitation.ExpiresAt, rawToken);
    }
}
