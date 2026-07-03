using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Application.Features.Notifications;

public class NotificationDto
{
    public Guid Id { get; init; }
    public NotificationType Type { get; init; }
    public string Text { get; init; } = string.Empty;
    public Guid? TargetId { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
