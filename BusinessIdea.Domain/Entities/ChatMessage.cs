using BusinessIdea.Domain.Common;

namespace BusinessIdea.Domain.Entities;

/// <summary>A single message inside an accepted conversation.</summary>
public class ChatMessage : BaseEntity, IAuditableEntity
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    /// <summary>Identity id of the sender (one of the two participants).</summary>
    public string SenderId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    /// <summary>Whether the other participant has seen the message.</summary>
    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
