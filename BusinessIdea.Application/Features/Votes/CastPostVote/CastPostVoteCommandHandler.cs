using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Votes.CastPostVote;

public class CastPostVoteCommandHandler : IRequestHandler<CastPostVoteCommand, VoteResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CastPostVoteCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VoteResultDto> Handle(CastPostVoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in to vote.");

        var postExists = await _context.BusinessIdeas
            .AnyAsync(i => i.Id == request.PostId, cancellationToken);
        if (!postExists)
        {
            throw new NotFoundException(nameof(BusinessIdeaPost), request.PostId);
        }

        var existing = await _context.PostVotes
            .FirstOrDefaultAsync(v => v.PostId == request.PostId && v.UserId == userId, cancellationToken);

        VoteDirection? resultingVote;

        if (existing is null)
        {
            _context.PostVotes.Add(new PostVote
            {
                PostId = request.PostId,
                UserId = userId,
                Direction = request.Direction
            });
            resultingVote = request.Direction;
        }
        else if (existing.Direction == request.Direction)
        {
            // Voting the same way again clears the vote (Reddit toggle behaviour).
            _context.PostVotes.Remove(existing);
            resultingVote = null;
        }
        else
        {
            existing.Direction = request.Direction;
            resultingVote = request.Direction;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var tallies = await _context.PostVotes
            .Where(v => v.PostId == request.PostId)
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
