using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Chat.Commands.SendChatMessage;

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ChatMessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public SendChatMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IRealtimeNotifier notifier)
    {
        _context = context;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<ChatMessageDto> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in to send messages.");

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        var isParticipant = conversation.RequesterId == userId || conversation.RecipientId == userId;
        if (!isParticipant)
        {
            throw new ForbiddenAccessException("You are not part of this conversation.");
        }

        if (conversation.Status != ChatRequestStatus.Accepted)
        {
            throw new ForbiddenAccessException("The chat request has not been accepted yet.");
        }

        var message = new ChatMessage
        {
            ConversationId = conversation.Id,
            SenderId = userId,
            Content = request.Content.Trim(),
        };
        _context.ChatMessages.Add(message);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAtUtc = message.CreatedAtUtc,
        };

        // Deliver instantly to the other participant's open connections.
        var otherUserId = conversation.RequesterId == userId
            ? conversation.RecipientId
            : conversation.RequesterId;
        await _notifier.SendToUserAsync(otherUserId, "chatMessage", dto, cancellationToken);

        return dto;
    }
}
