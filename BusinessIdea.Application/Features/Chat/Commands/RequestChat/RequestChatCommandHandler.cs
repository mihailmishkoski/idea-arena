using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Notifications;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = BusinessIdea.Application.Common.Exceptions.ValidationException;

namespace BusinessIdea.Application.Features.Chat.Commands.RequestChat;

public class RequestChatCommandHandler : IRequestHandler<RequestChatCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public RequestChatCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IRealtimeNotifier notifier)
    {
        _context = context;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<Guid> Handle(RequestChatCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in to start a chat.");

        if (request.RecipientId == userId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.RecipientId), "You cannot start a chat with yourself."),
            });
        }

        var recipientExists = await _context.Authors
            .AnyAsync(u => u.Id == request.RecipientId, cancellationToken);
        if (!recipientExists)
        {
            throw new NotFoundException("User", request.RecipientId);
        }

        // Reuse an existing (pending or accepted) conversation in either direction.
        var existing = await _context.Conversations
            .Where(c => c.Status != ChatRequestStatus.Declined)
            .Where(c =>
                (c.RequesterId == userId && c.RecipientId == request.RecipientId) ||
                (c.RequesterId == request.RecipientId && c.RecipientId == userId))
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is { } existingId)
        {
            return existingId;
        }

        var conversation = new Conversation
        {
            RequesterId = userId,
            RecipientId = request.RecipientId,
            PostId = request.PostId,
        };
        _context.Conversations.Add(conversation);

        var requesterName = await _context.Authors
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? "Someone";

        var notification = new Notification
        {
            UserId = request.RecipientId,
            Type = NotificationType.ChatRequest,
            Text = $"{requesterName} wants to chat with you.",
            TargetId = conversation.Id,
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync(cancellationToken);

        await _notifier.SendToUserAsync(request.RecipientId, "notification", new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Text = notification.Text,
            TargetId = notification.TargetId,
            IsRead = false,
            CreatedAtUtc = notification.CreatedAtUtc,
        }, cancellationToken);

        return conversation.Id;
    }
}
