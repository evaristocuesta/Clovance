using System.Net.Mail;

namespace Clovance.ApiService.Infrastructure.Email;

public static class SmtpEmailExtension
{
    public static IServiceCollection AddSmtpEmailSender(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .Validate(
                options => IsNotConfigured(options) || HasCompleteConfiguration(options),
                $"{SmtpOptions.SectionName}: SMTP can be empty, but if you configure it, Host, FromAddress, Username and Password are all required.")
            .Validate(
                options => IsNotConfigured(options) || options.Port is >= 1 and <= 65_535,
                $"{SmtpOptions.SectionName}: Port must be between 1 and 65535.")
            .Validate(
                options => IsNotConfigured(options) || MailAddress.TryCreate(options.FromAddress, out _),
                $"{SmtpOptions.SectionName}: FromAddress must be a valid email address.")
            .ValidateOnStart();

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }

    private static bool IsNotConfigured(SmtpOptions options)
        => string.IsNullOrWhiteSpace(options.Host)
           && string.IsNullOrWhiteSpace(options.FromAddress)
           && string.IsNullOrWhiteSpace(options.Username)
           && string.IsNullOrWhiteSpace(options.Password);

    private static bool HasCompleteConfiguration(SmtpOptions options)
        => !string.IsNullOrWhiteSpace(options.Host)
           && !string.IsNullOrWhiteSpace(options.FromAddress)
           && !string.IsNullOrWhiteSpace(options.Username)
           && !string.IsNullOrWhiteSpace(options.Password);
}
