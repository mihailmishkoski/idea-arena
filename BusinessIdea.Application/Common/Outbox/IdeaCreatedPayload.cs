namespace BusinessIdea.Application.Common.Outbox;

/// <summary>Payload of <see cref="OutboxEventTypes.IdeaCreated"/>.</summary>
public record IdeaCreatedPayload(Guid PostId);
