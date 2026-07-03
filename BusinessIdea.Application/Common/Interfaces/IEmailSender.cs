using BusinessIdea.Application.Common.Models;

namespace BusinessIdea.Application.Common.Interfaces;

/// <summary>
/// Outgoing-email port. The Application layer composes messages; the transport
/// (SMTP, provider API…) is an Infrastructure concern behind this interface.
/// Implementations should throw on failure so the outbox retry kicks in.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
