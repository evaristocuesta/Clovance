namespace Clovance.ApiService.Infrastructure.Email;

public sealed class NoOpEmailSender : IEmailSender
{
    public bool IsConfigured => true;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
