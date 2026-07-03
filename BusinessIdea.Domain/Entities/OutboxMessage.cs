using BusinessIdea.Domain.Common;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// Transactional-outbox record. Command handlers append one of these in the
/// same transaction as the business change they describe; a background worker
/// later picks it up and performs the side effect (email, AI enrichment).
/// This guarantees the event exists if-and-only-if the business change was
/// committed — no "email claimed sent but nothing happened" inconsistencies.
/// </summary>
public class OutboxMessage : BaseEntity, IAuditableEntity
{
    /// <summary>Discriminator the worker dispatches on (e.g. "chat-requested").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>JSON payload; kept minimal (ids), the worker re-reads fresh data.</summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>Set when the worker completed the side effect successfully.</summary>
    public DateTimeOffset? ProcessedAtUtc { get; set; }

    /// <summary>How many times processing has been attempted (for retry/backoff).</summary>
    public int Attempts { get; set; }

    /// <summary>Last failure, kept for diagnosis; cleared on success.</summary>
    public string? LastError { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
