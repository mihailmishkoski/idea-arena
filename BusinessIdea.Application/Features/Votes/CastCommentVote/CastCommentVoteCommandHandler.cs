using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Votes.CastCommentVote;

public class CastCommentVoteCommandHandler : IRequestHandler<CastCommentVoteCommand, VoteResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CastCommentVoteCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VoteResultDto> Handle(CastCommentVoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in to vote.");

        var commentExists = await _context.Comments
            .AnyAsync(c => c.Id == request.CommentId, cancellationToken);
        if (!commentExists)
        {
            throw new NotFoundException(nameof(Comment), request.CommentId);
        }

        var existing = await _context.CommentVotes
            .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.UserId == userId, cancellationToken);

        VoteDirection? resultingVote;

        if (existing is null)
        {
            _context.CommentVotes.Add(new CommentVote
            {
                CommentId = request.CommentId,
                UserId = userId,
                Direction = request.Direction
            });
            resultingVote = request.Direction;
        }
        else if (existing.Direction == request.Direction)
        {
            _context.CommentVotes.Remove(existing);
            resultingVote = null;
        }
        else
        {
            existing.Direction = request.Direction;
            resultingVote = request.Direction;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var tallies = await _context.CommentVotes
            .Where(v => v.CommentId == request.CommentId)
            .GroupBy(v => v.Direction)
            .Select(g => new { Direction = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new VoteResultDto
        {
            UpVotes = tallies.FirstOrDefault(t => t.Direction == VoteDirection.Up)?.Count ?? 0,
            DownVotes = tallies.FirstOrDefault(t => t.Direction == VoteDirection.Down)?.Count ?? 0,
            CurrentUserVote = resultingVote
        };
    }
}
