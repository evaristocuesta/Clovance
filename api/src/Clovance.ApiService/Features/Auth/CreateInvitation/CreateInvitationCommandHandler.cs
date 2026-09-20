using System.Security.Claims;
using Clovance.ApiService.Domain.UserInvitations;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Auth.Token;
using Clovance.ApiService.Infrastructure.Auth.UserInvitation;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.Email;
using Clovance.ApiService.Infrastructure.Frontend;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationCommandHandler : IHandler<CreateInvitationCommand, Result<CreateInvitationResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IStringLocalizer<EmailResources> _localizer;
    private readonly UserInvitationOptions _invitationOptions;
    private readonly FrontendOptions _frontendOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateInvitationCommandHandler(
        UserManager<ApplicationUser> userManager,
        ClovanceDbContext dbContext,
        ITokenService tokenService,
        IEmailSender emailSender,
        IStringLocalizer<EmailResources> localizer,
        IOptions<UserInvitationOptions> invitationOptions,
        IOptions<FrontendOptions> frontendOptions,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _localizer = localizer;
        _invitationOptions = invitationOptions.Value;
        _frontendOptions = frontendOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<CreateInvitationResult>> HandleAsync(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!_emailSender.IsConfigured)
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.EmailNotConfigured());
        }

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
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Email == email && i.ConsumedAt == null && i.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);

        if (activeInvitation is not null)
        {
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.ActiveInvitationAlreadyExists());
        }

        var rawToken = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(rawToken);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(Math.Max(1, _invitationOptions.ExpirationHours));

        var invitation = UserInvitation.Create(email.Value, request.IsAdmin, tokenHash, expiresAt, userId);

        await _dbContext.UserInvitations.AddAsync(invitation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailSender.SendAsync(BuildInvitationEmail(email.Value, rawToken), cancellationToken);
        }
        catch (Exception ex)
        {
            _dbContext.UserInvitations.Remove(invitation);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<CreateInvitationResult>.Failure(AppErrors.Auth.EmailSendFailed(ex.Message));
        }

        return Result<CreateInvitationResult>.Success(new CreateInvitationResult(invitation.Id.Value, invitation.Email.Value, invitation.ExpiresAt, rawToken));
    }

    private EmailMessage BuildInvitationEmail(string toEmail, string token)
    {
        var invitationLink = $"{_frontendOptions.BaseUrl}/auth/register?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";

        var htmlBody = $"""
        <p>{_localizer["Invitation_Intro"]}</p>
        <p><a href="{invitationLink}">{_localizer["Invitation_LinkText"]}</a></p>
        <p>{_localizer["Invitation_Expiration", _invitationOptions.ExpirationHours]}</p>
        <p>{_localizer["Invitation_Ignore"]}</p>
        """;

        return new EmailMessage(
            To: toEmail,
            Subject: _localizer["Invitation_Subject"],
            HtmlBody: htmlBody);
    }
}
