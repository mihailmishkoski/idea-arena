using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Chat.Queries.GetConversations;

public class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, IReadOnlyCollection<ConversationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConversationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ConversationDto>> Handle(
        GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in.");

        var conversations = await _context.Conversations
            .AsNoTracking()
            .Where(c => c.RequesterId == userId || c.RecipientId == userId)
            .Select(c => new
            {
                c.Id,
                c.Status,
                c.RequesterId,
                c.PostId,
                c.CreatedAtUtc,
                OtherUserId = c.RequesterId == userId ? c.RecipientId : c.RequesterId,
                OtherUser = _context.Authors
                    .Where(u => u.Id == (c.RequesterId == userId ? c.RecipientId : c.RequesterId))
                    .Select(u => new { u.DisplayName, u.AvatarId })
                    .FirstOrDefault(),
                PostName = c.PostId == null
                    ? null
                    : _context.BusinessIdeas
                        .Where(p => p.Id == c.PostId)
                        .Select(p => p.Name)
                        .FirstOrDefault(),
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAtUtc)
                    .Select(m => new { m.Content, m.CreatedAtUtc })
                    .FirstOrDefault(),
                UnreadCount = c.Messages.Count(m => m.SenderId != userId && !m.IsRead),
            })
            .ToListAsync(cancellationToken);

        return conversations
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                Status = c.Status,
                IAmRequester = c.RequesterId == userId,
                OtherUserId = c.OtherUserId,
                OtherUserName = c.OtherUser?.DisplayName,
                OtherUserAvatar = c.OtherUser?.AvatarId,
                PostId = c.PostId,
                PostName = c.PostName,
                LastMessage = c.LastMessage?.Content,
                LastMessageAtUtc = c.LastMessage?.CreatedAtUtc,
                CreatedAtUtc = c.CreatedAtUtc,
                UnreadCount = c.UnreadCount,
            })
            .OrderByDescending(c => c.LastMessageAtUtc ?? c.CreatedAtUtc)
            .ToList();
    }
}
