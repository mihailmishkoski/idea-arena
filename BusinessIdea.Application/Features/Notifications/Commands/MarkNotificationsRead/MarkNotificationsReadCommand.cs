using MediatR;

namespace BusinessIdea.Application.Features.Notifications.Commands.MarkNotificationsRead;

/// <summary>Marks all of the current user's notifications as read.</summary>
public record MarkNotificationsReadCommand : IRequest;
