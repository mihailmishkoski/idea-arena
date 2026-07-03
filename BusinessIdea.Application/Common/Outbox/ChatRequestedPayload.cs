namespace BusinessIdea.Application.Common.Outbox;

/// <summary>Payload of <see cref="OutboxEventTypes.ChatRequested"/>.</summary>
public record ChatRequestedPayload(Guid ConversationId);
