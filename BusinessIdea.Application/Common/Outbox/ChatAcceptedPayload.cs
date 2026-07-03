namespace BusinessIdea.Application.Common.Outbox;

/// <summary>Payload of <see cref="OutboxEventTypes.ChatAccepted"/>.</summary>
public record ChatAcceptedPayload(Guid ConversationId);
