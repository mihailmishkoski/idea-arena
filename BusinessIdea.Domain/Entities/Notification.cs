using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// An in-app notification for a user (comment reply, chat request, …). The
/// <see cref="TargetId"/> points at the thing to open — a post for comment
/// notifications, a conversation for chat ones.
/// </summary>
public class Notification : BaseEntity, IAuditableEntity
{
    /// <summary>Identity id of the user who receives the notification.</summary>
    public string UserId { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public string Text { get; set; } = string.Empty;

    public Guid? TargetId { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
