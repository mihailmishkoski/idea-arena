#nullable enable
namespace BusinessIdea.Application.Tests.Features.Notifications.Commands.MarkNotificationsReadCommandTests;

using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;

public static class MarkNotificationsReadCommandTestsHelper
{
    public static readonly string UserId = "user-1";
    public static readonly string OtherUserId = "other-user";

    public static Notification GetNotification(string userId, bool isRead)
    {
        return new Notification
        {
            UserId = userId,
            Type = NotificationType.ChatRequest,
            Text = "Someone wants to chat with you.",
            IsRead = isRead,
        };
    }
}
