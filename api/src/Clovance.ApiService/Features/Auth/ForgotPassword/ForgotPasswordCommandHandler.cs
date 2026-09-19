using Clovance.ApiService.Features.Auth.ForgotPassword;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.Email;
using Clovance.ApiService.Infrastructure.Frontend;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

public sealed class ForgotPasswordCommandHandler : IHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly FrontendOptions _frontendOptions;
    private readonly IStringLocalizer<EmailResources> _localizer;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IOptions<FrontendOptions> frontendOptions,
        IStringLocalizer<EmailResources> localizer,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _frontendOptions = frontendOptions.Value;
        _localizer = localizer;
        _logger = logger;
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

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        try
        {
            await _emailSender.SendAsync(BuildResetEmail(user.Email!, token), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(AppErrors.Auth.EmailSendFailed(ex.Message));
        }

        return Result.Success();
    }

    private EmailMessage BuildResetEmail(string toEmail, string token)
    {
        var resetLink = $"{_frontendOptions.BaseUrl}/auth/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";

        var htmlBody = $"""
            <p>{_localizer["PasswordReset_Intro"]}</p>
            <p><a href="{resetLink}">{_localizer["PasswordReset_LinkText"]}</a></p>
            <p>{_localizer["PasswordReset_Ignore"]}</p>
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: _localizer["PasswordReset_Subject"],
            HtmlBody: htmlBody);
    }
}
