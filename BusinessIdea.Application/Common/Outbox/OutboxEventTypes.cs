namespace BusinessIdea.Application.Common.Outbox;

/// <summary>
/// Discriminators for outbox messages. The worker dispatches on these strings,
/// so they are the contract between the API process and the worker process.
/// </summary>
public static class OutboxEventTypes
{
    /// <summary>A new idea was posted → the AI critic seeds tough questions.</summary>
    public const string IdeaCreated = "idea-created";

    /// <summary>A chat / co-founder request was created → email the recipient.</summary>
    public const string ChatRequested = "chat-requested";

    /// <summary>A chat request was accepted → email the requester.</summary>
    public const string ChatAccepted = "chat-accepted";

    /// <summary>A co-founder application was submitted → rich email to the idea's author.</summary>
    public const string CofounderApplied = "cofounder-applied";
}
