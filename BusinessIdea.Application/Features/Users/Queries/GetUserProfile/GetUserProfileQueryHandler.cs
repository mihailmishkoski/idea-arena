using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.BusinessIdeas.Queries;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private const int RecentIdeasCount = 10;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserProfileQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var member = await _context.Authors
            .Where(u => u.Id == request.UserId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        var ideasCount = await _context.BusinessIdeas
            .CountAsync(i => i.AuthorId == request.UserId, cancellationToken);

        var commentsCount = await _context.Comments
            .CountAsync(c => c.AuthorId == request.UserId, cancellationToken);

        var postKarma = await _context.PostVotes
            .Where(v => v.Post.AuthorId == request.UserId)
            .SumAsync(v => v.Direction == VoteDirection.Up ? 1 : -1, cancellationToken);

        var commentKarma = await _context.CommentVotes
            .Where(v => v.Comment.AuthorId == request.UserId)
            .SumAsync(v => v.Direction == VoteDirection.Up ? 1 : -1, cancellationToken);

        var wins = await _context.WeeklyWinners
            .CountAsync(w => w.AuthorId == request.UserId, cancellationToken);

        var viewerId = _currentUser.UserId;
        var recentIdeas = await _context.BusinessIdeas
            .AsNoTracking()
            .Where(i => i.AuthorId == request.UserId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .Take(RecentIdeasCount)
            .Select(i => new BusinessIdeaSummaryDto
            {
                Id = i.Id,
                Name = i.Name,
                UniqueValueProposition = i.UniqueValueProposition,
                AuthorId = i.AuthorId,
                AuthorName = member.DisplayName,
                AuthorAvatar = member.AvatarId,
                CreatedAtUtc = i.CreatedAtUtc,
                UpVotes = i.Votes.Count(v => v.Direction == VoteDirection.Up),
                DownVotes = i.Votes.Count(v => v.Direction == VoteDirection.Down),
                CommentCount = i.Comments.Count,
                CurrentUserVote = viewerId == null
                    ? null
                    : i.Votes
                        .Where(v => v.UserId == viewerId)
                        .Select(v => (VoteDirection?)v.Direction)
                        .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return new UserProfileDto
        {
            Id = member.Id,
            DisplayName = member.DisplayName,
            AvatarId = member.AvatarId,
            IdeasCount = ideasCount,
            CommentsCount = commentsCount,
            Karma = postKarma + commentKarma,
            Wins = wins,
            RecentIdeas = recentIdeas,
        };
    }
}
