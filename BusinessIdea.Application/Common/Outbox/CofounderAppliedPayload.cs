namespace BusinessIdea.Application.Common.Outbox;

/// <summary>
/// Payload of <see cref="OutboxEventTypes.CofounderApplied"/>. Carries the
/// composed application text itself — it exists nowhere else: in-app the
/// author only sees a plain accept/decline request, the details go by email.
/// </summary>
public record CofounderAppliedPayload(Guid ConversationId, string ApplicationText);
