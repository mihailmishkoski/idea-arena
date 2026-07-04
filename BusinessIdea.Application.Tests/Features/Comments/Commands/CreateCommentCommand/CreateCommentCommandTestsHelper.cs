#nullable enable
namespace BusinessIdea.Application.Tests.Features.Comments.Commands.CreateCommentCommandTests;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Features.Comments.Commands.CreateComment;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class CreateCommentCommandTestsHelper
{
    public static readonly string CommenterId = "commenter-1";
    public static readonly string PostAuthorId = "post-author";
    public static readonly string CommenterName = "Ava";
    public static readonly string PostName = "NovaFlow";
    public static readonly string Content = "  Great idea!  ";

    public static BusinessIdeaPost GetIdea(Guid postId)
    {
        return new BusinessIdeaPost
        {
            Id = postId,
            Name = PostName,
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = PostAuthorId,
        };
    }

    public static Comment GetParentComment(Guid parentId, Guid postId)
    {
        return new Comment
        {
            Id = parentId,
            PostId = postId,
            AuthorId = PostAuthorId,
            Content = "Parent comment",
            TargetMetric = IdeaMetric.ExitStrategy,
        };
    }

    public static AuthorInfo GetCommenter()
    {
        return new AuthorInfo
        {
            Id = CommenterId,
            DisplayName = CommenterName,
        };
    }

    public static CreateCommentCommand GetCommand(Guid postId, Guid? parentCommentId = null)
    {
        return new CreateCommentCommand
        {
            PostId = postId,
            Content = Content,
            TargetMetric = IdeaMetric.Problem,
            ParentCommentId = parentCommentId,
        };
    }
}
