using MediatR;

namespace BusinessIdea.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>The current user's most recent notifications, newest first.</summary>
public record GetNotificationsQuery(int Limit = 30) : IRequest<IReadOnlyCollection<NotificationDto>>;
