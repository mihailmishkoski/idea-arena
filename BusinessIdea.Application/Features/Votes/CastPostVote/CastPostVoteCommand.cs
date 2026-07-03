using BusinessIdea.Domain.Enums;
using MediatR;

namespace BusinessIdea.Application.Features.Votes.CastPostVote;

/// <summary>
/// Casts (or toggles) the current user's vote on an idea. Voting the same
/// direction twice clears the vote; voting the opposite direction flips it.
/// </summary>
public record CastPostVoteCommand(Guid PostId, VoteDirection Direction) : IRequest<VoteResultDto>;
