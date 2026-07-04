#nullable enable
namespace BusinessIdea.Application.Tests.Features.Votes.CastCommentVoteCommandTests;

using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class CastCommentVoteCommandTestsHelper
{
    public static readonly string UserId = "voter-1";

    public static Comment GetComment(Guid commentId)
    {
        return new Comment
        {
            Id = commentId,
            PostId = Guid.NewGuid(),
            AuthorId = "comment-author",
            Content = "A comment",
        };
    }

    public static CommentVote GetVote(Guid commentId, VoteDirection direction)
    {
        return new CommentVote
        {
            CommentId = commentId,
            UserId = UserId,
            Direction = direction,
        };
    }
}
