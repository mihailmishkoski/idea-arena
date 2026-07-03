using BusinessIdea.Domain.Entities;

namespace BusinessIdea.Application.Features.Outbox;

/// <summary>
/// Handles one outbox event type. The worker resolves all registered
/// processors and dispatches each pending message to the one whose
/// <see cref="Type"/> matches. Throwing leaves the message unprocessed so the
/// worker retries it later (at-least-once delivery).
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>The <see cref="Common.Outbox.OutboxEventTypes"/> value this handles.</summary>
    string Type { get; }

    Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken);
}
