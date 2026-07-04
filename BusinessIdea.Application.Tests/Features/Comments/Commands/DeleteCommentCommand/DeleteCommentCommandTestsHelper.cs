#nullable enable
namespace BusinessIdea.Application.Tests.Features.Comments.Commands.DeleteCommentCommandTests;

using BusinessIdea.Domain.Entities;
using System;

public static class DeleteCommentCommandTestsHelper
{
    public static readonly string AuthorId = "comment-author";
    public static readonly string OtherUserId = "other-user";

    public static Comment GetComment(Guid commentId)
    {
        return new Comment
        {
            Id = commentId,
            PostId = Guid.NewGuid(),
            AuthorId = AuthorId,
            Content = "A comment",
        };
    }
}
