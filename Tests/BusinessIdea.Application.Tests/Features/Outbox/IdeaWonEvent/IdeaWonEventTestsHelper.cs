#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.IdeaWonEvent;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using System;

public static class IdeaWonEventTestsHelper
{
    public static readonly string AuthorId = "idea-author";
    public static readonly string AuthorEmail = "author@example.com";
    public static readonly string PostName = "NovaFlow";

    public static PublicAppUrls GetUrls()
    {
        return new PublicAppUrls { BaseUrl = "http://localhost:4200" };
    }

    public static WeeklyWinner GetWinner(Guid winnerId, Guid? postId)
    {
        return new WeeklyWinner
        {
            Id = winnerId,
            PeriodStartUtc = DateTimeOffset.UtcNow.AddDays(-7),
            PeriodEndUtc = DateTimeOffset.UtcNow,
            PostId = postId,
            PostName = PostName,
            AuthorId = AuthorId,
            UpVotes = 7,
            DownVotes = 2,
            Score = 5,
            CommentCount = 4,
        };
    }

    public static AuthorInfo GetAuthor(string? email)
    {
        return new AuthorInfo { Id = AuthorId, DisplayName = "Ava", Email = email };
    }

    public static OutboxMessage GetMessage(Guid winnerId)
    {
        return OutboxMessageFactory.Create(OutboxEventTypes.IdeaWon, new IdeaWonPayload(winnerId));
    }
}
