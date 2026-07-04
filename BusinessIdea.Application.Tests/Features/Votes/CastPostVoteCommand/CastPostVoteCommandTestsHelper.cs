#nullable enable
namespace BusinessIdea.Application.Tests.Features.Votes.CastPostVoteCommandTests;

using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class CastPostVoteCommandTestsHelper
{
    public static readonly string UserId = "voter-1";

    public static BusinessIdeaPost GetIdea(Guid postId)
    {
        return new BusinessIdeaPost
        {
            Id = postId,
            Name = "Idea",
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = "post-author",
        };
    }

    public static PostVote GetVote(Guid postId, VoteDirection direction)
    {
        return new PostVote
        {
            PostId = postId,
            UserId = UserId,
            Direction = direction,
        };
    }
}
