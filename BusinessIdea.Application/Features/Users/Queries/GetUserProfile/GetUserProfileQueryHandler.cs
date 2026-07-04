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
        var viewerId = _currentUser.UserId;

        var profile = await _context.Authors
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                AvatarId = u.AvatarId,
                IdeasCount = _context.BusinessIdeas.Count(i => i.AuthorId == u.Id),
                CommentsCount = _context.Comments.Count(c => c.AuthorId == u.Id),
                Karma =
                    _context.PostVotes
                        .Where(v => v.Post.AuthorId == u.Id)
                        .Sum(v => v.Direction == VoteDirection.Up ? 1 : -1) +
                    _context.CommentVotes
                        .Where(v => v.Comment.AuthorId == u.Id)
                        .Sum(v => v.Direction == VoteDirection.Up ? 1 : -1),
                Wins = _context.WeeklyWinners.Count(w => w.AuthorId == u.Id),
                RecentIdeas = _context.BusinessIdeas
                    .Where(i => i.AuthorId == u.Id)
                    .OrderByDescending(i => i.CreatedAtUtc)
                    .Take(RecentIdeasCount)
                    .Select(i => new BusinessIdeaSummaryDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        UniqueValueProposition = i.UniqueValueProposition,
                        AuthorId = i.AuthorId,
                        AuthorName = u.DisplayName,
                        AuthorAvatar = u.AvatarId,
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
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return profile ?? throw new NotFoundException("User", request.UserId);
    }
}
