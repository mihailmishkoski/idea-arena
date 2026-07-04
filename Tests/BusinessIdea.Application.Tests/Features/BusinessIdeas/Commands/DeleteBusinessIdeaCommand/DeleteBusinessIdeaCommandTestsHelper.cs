#nullable enable
namespace BusinessIdea.Application.Tests.Features.BusinessIdeas.Commands.DeleteBusinessIdeaCommandTests;

using BusinessIdea.Domain.Entities;
using System;

public static class DeleteBusinessIdeaCommandTestsHelper
{
    public static readonly string AuthorId = "author-1";
    public static readonly string OtherUserId = "other-user";

    public static BusinessIdeaPost GetIdea(Guid ideaId)
    {
        return new BusinessIdeaPost
        {
            Id = ideaId,
            Name = "Idea to delete",
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = AuthorId,
        };
    }
}
