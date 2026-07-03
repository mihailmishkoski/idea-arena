using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Chat.Queries.GetConversationMessages;

public class GetConversationMessagesQueryHandler
    : IRequestHandler<GetConversationMessagesQuery, IReadOnlyCollection<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConversationMessagesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ChatMessageDto>> Handle(
        GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in.");

        var isParticipant = await _context.Conversations
            .AnyAsync(
                c => c.Id == request.ConversationId &&
                     (c.RequesterId == userId || c.RecipientId == userId),
                cancellationToken);

        if (!isParticipant)
        {
            throw new NotFoundException(nameof(Conversation), request.ConversationId);
        }

        var messages = await _context.ChatMessages
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        // Opening the thread marks everything from the other side as read.
        var becameRead = false;
        foreach (var message in messages.Where(m => m.SenderId != userId && !m.IsRead))
        {
            message.IsRead = true;
            becameRead = true;
        }

        if (becameRead)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return messages
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                SentAtUtc = m.CreatedAtUtc,
            })
            .ToList();
    }
}
