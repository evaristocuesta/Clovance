using Clovance.ApiService.Domain.PasswordResetTokens;
using Clovance.ApiService.Features.Auth.ForgotPassword;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Auth.PasswordReset;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.Email;
using Clovance.ApiService.Infrastructure.Frontend;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

public sealed class ForgotPasswordCommandHandler : IHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly FrontendOptions _frontendOptions;
    private readonly PasswordResetOptions _passwordResetOptions;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly IStringLocalizer<EmailResources> _localizer;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ClovanceDbContext dbContext,
        IEmailSender emailSender,
        IOptions<FrontendOptions> frontendOptions,
        IOptions<PasswordResetOptions> passwordResetOptions,
        ILogger<ForgotPasswordCommandHandler> logger,
        IStringLocalizer<EmailResources> localizer)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _emailSender = emailSender;
        _frontendOptions = frontendOptions.Value;
        _passwordResetOptions = passwordResetOptions.Value;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result> HandleAsync(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        if (!_emailSender.IsConfigured)
        {
            return Result.Failure(AppErrors.Auth.EmailNotConfigured());
        }

        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user is null)
        {
            _logger.LogInformation("Password reset requested for a non-existing email.");
            return Result.Success();
        }

        var existingTokens = await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == PasswordResetTokenUserId.Create(user.Id) && !t.IsUsed)
            .ToListAsync(cancellationToken);

        _dbContext.PasswordResetTokens.RemoveRange(existingTokens);

        var (token, plainTextToken) = PasswordResetToken.Create(
            user.Id,
            TimeSpan.FromMinutes(_passwordResetOptions.ExpirationMinutes));

        await _dbContext.PasswordResetTokens.AddAsync(token, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailSender.SendAsync(BuildResetEmail(user.Email!, plainTextToken), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email.");
        }

        return Result.Success();
    }

    private EmailMessage BuildResetEmail(string toEmail, string plainTextToken)
    {
        var resetLink = $"{_frontendOptions.BaseUrl}/reset-password?token={Uri.EscapeDataString(plainTextToken)}";

        var htmlBody = $"""
            <p>{_localizer["PasswordReset_Intro"]}</p>
            <p><a href="{resetLink}">{_localizer["PasswordReset_LinkText"]}</a></p>
            <p>{_localizer["PasswordReset_Expiration", _passwordResetOptions.ExpirationMinutes]}</p>
            <p>{_localizer["PasswordReset_Ignore"]}</p>
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: _localizer["PasswordReset_Subject"],
            HtmlBody: htmlBody);
    }
}
