#nullable enable
namespace BusinessIdea.Application.Tests.Features.BusinessIdeas.Commands.UpdateBusinessIdeaCommandTests;

using BusinessIdea.Application.Features.BusinessIdeas.Commands.UpdateBusinessIdea;
using BusinessIdea.Domain.Entities;
using System;

public static class UpdateBusinessIdeaCommandTestsHelper
{
    public static readonly string AuthorId = "author-1";
    public static readonly string OtherUserId = "other-user";
    public static readonly string UpdatedName = "  Renamed idea  ";

    public static BusinessIdeaPost GetIdea(Guid ideaId)
    {
        return new BusinessIdeaPost
        {
            Id = ideaId,
            Name = "Original name",
            UniqueValueProposition = "Original UVP",
            Problem = "Original problem",
            Solution = "Original solution",
            AuthorId = AuthorId,
        };
    }

    public static UpdateBusinessIdeaCommand GetCommand(Guid ideaId)
    {
        return new UpdateBusinessIdeaCommand
        {
            Id = ideaId,
            Name = UpdatedName,
            UniqueValueProposition = "New UVP",
            Problem = "New problem",
            Solution = "New solution",
        };
    }
}
