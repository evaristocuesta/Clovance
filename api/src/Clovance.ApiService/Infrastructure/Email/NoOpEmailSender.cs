using System.Collections.Concurrent;

namespace Clovance.ApiService.Infrastructure.Email;

/// <summary>
/// No-op sender used in the Testing environment. It keeps the messages in memory so
/// integration tests can read tokens that are only delivered by email.
/// </summary>
public sealed class NoOpEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _messages = new();

    public bool IsConfigured => true;

    /// <summary>
    /// Every message sent so far, in order. Read by tests to obtain invitation and
    /// password reset links from the email body.
    /// </summary>
    public IReadOnlyList<EmailMessage> Messages => _messages.ToArray();

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }
}
