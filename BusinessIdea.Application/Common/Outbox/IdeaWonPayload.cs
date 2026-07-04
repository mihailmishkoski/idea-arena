namespace BusinessIdea.Application.Common.Outbox;

/// <summary>Carried by the "idea-won" outbox event; the row holds the snapshot.</summary>
public record IdeaWonPayload(Guid WinnerId);
