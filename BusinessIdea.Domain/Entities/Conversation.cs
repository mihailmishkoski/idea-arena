using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// A chat between two users. It starts as a request from the requester to the
/// recipient (usually the author of an idea); messages can only flow once the
/// recipient accepts.
/// </summary>
public class Conversation : BaseEntity, IAuditableEntity
{
    /// <summary>Identity id of the user who initiated the chat.</summary>
    public string RequesterId { get; set; } = string.Empty;

    /// <summary>Identity id of the user being contacted.</summary>
    public string RecipientId { get; set; } = string.Empty;

    /// <summary>The idea that prompted the request, when started from a post.</summary>
    public Guid? PostId { get; set; }

    public ChatRequestStatus Status { get; set; } = ChatRequestStatus.Pending;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
