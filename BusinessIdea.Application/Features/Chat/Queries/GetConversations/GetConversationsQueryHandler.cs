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
                c.RecipientId,
                c.PostId,
                c.CreatedAtUtc,
                OtherUserId = c.RequesterId == userId ? c.RecipientId : c.RequesterId,
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAtUtc)
                    .Select(m => new { m.Content, m.CreatedAtUtc })
                    .FirstOrDefault(),
                UnreadCount = c.Messages.Count(m => m.SenderId != userId && !m.IsRead),
            })
            .ToListAsync(cancellationToken);

        var otherIds = conversations.Select(c => c.OtherUserId).Distinct().ToList();
        var authors = await _context.Authors
            .Where(u => otherIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var postIds = conversations.Where(c => c.PostId != null).Select(c => c.PostId!.Value).ToList();
        var postNames = await _context.BusinessIdeas
            .Where(p => postIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        return conversations
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                Status = c.Status,
                IAmRequester = c.RequesterId == userId,
                OtherUserId = c.OtherUserId,
                OtherUserName = authors.TryGetValue(c.OtherUserId, out var author) ? author.DisplayName : null,
                OtherUserAvatar = authors.TryGetValue(c.OtherUserId, out var a) ? a.AvatarId : null,
                PostId = c.PostId,
                PostName = c.PostId != null && postNames.TryGetValue(c.PostId.Value, out var name) ? name : null,
                LastMessage = c.LastMessage?.Content,
                LastMessageAtUtc = c.LastMessage?.CreatedAtUtc,
                CreatedAtUtc = c.CreatedAtUtc,
                UnreadCount = c.UnreadCount,
            })
            .OrderByDescending(c => c.LastMessageAtUtc ?? c.CreatedAtUtc)
            .ToList();
    }
}
