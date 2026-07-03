using BusinessIdea.Domain.Enums;
using MediatR;

namespace BusinessIdea.Application.Features.Votes.CastCommentVote;

/// <summary>
/// Casts (or toggles) the current user's vote on a comment. Same toggle
/// semantics as voting on an idea.
/// </summary>
public record CastCommentVoteCommand(Guid CommentId, VoteDirection Direction) : IRequest<VoteResultDto>;
