using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Comments.Queries.GetPostComments;

public class GetPostCommentsQueryHandler
    : IRequestHandler<GetPostCommentsQuery, IReadOnlyCollection<CommentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPostCommentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<CommentDto>> Handle(
        GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.PostId == request.PostId);

        if (request.TargetMetric is not null)
        {
            query = query.Where(c => c.TargetMetric == request.TargetMetric);
        }

        return await query
            .Select(c => new CommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                ParentCommentId = c.ParentCommentId,
                AuthorId = c.AuthorId,
                AuthorName = _context.Authors
                    .Where(u => u.Id == c.AuthorId)
                    .Select(u => u.DisplayName)
                    .FirstOrDefault(),
                AuthorAvatar = _context.Authors
                    .Where(u => u.Id == c.AuthorId)
                    .Select(u => u.AvatarId)
                    .FirstOrDefault(),
                Content = c.Content,
                TargetMetric = c.TargetMetric,
                CreatedAtUtc = c.CreatedAtUtc,
                UpVotes = c.Votes.Count(v => v.Direction == VoteDirection.Up),
                DownVotes = c.Votes.Count(v => v.Direction == VoteDirection.Down),
                CurrentUserVote = userId == null
                    ? null
                    : c.Votes
                        .Where(v => v.UserId == userId)
                        .Select(v => (VoteDirection?)v.Direction)
                        .FirstOrDefault()
            })
            .OrderByDescending(c => c.UpVotes - c.DownVotes)
            .ThenByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
