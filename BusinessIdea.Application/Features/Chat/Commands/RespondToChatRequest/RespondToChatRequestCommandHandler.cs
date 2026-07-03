using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Notifications;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Chat.Commands.RespondToChatRequest;

public class RespondToChatRequestCommandHandler : IRequestHandler<RespondToChatRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public RespondToChatRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IRealtimeNotifier notifier)
    {
        _context = context;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task Handle(RespondToChatRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in.");

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        if (conversation.RecipientId != userId)
        {
            throw new ForbiddenAccessException("Only the recipient can respond to a chat request.");
        }

        if (conversation.Status != ChatRequestStatus.Pending)
        {
            return; // Already answered — nothing to do.
        }

        conversation.Status = request.Accept ? ChatRequestStatus.Accepted : ChatRequestStatus.Declined;

        if (request.Accept)
        {
            var recipientName = await _context.Authors
                .Where(u => u.Id == userId)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync(cancellationToken) ?? "Someone";

            var notification = new Notification
            {
                UserId = conversation.RequesterId,
                Type = NotificationType.ChatAccepted,
                Text = $"{recipientName} accepted your chat request.",
                TargetId = conversation.Id,
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(cancellationToken);

            await _notifier.SendToUserAsync(conversation.RequesterId, "notification", new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Text = notification.Text,
                TargetId = notification.TargetId,
                IsRead = false,
                CreatedAtUtc = notification.CreatedAtUtc,
            }, cancellationToken);
        }
        else
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
