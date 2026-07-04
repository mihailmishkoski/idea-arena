#nullable enable
namespace BusinessIdea.Application.Tests.Features.Winners;

using BusinessIdea.Application.Features.Winners;
using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

public static class WinnerDeclarationServiceTestsHelper
{
    public static readonly string AuthorId = "idea-author";

    /// <summary>Start of the last fully completed competition week.</summary>
    public static DateTimeOffset LastCompletedWeekStart =>
        WinnerDeclarationService.WeekStartUtc(DateTimeOffset.UtcNow).AddDays(-7);

    /// <summary>
    /// An idea whose 14-day window closed inside the given week, carrying the
    /// requested number of up-votes.
    /// </summary>
    public static BusinessIdeaPost GetExpiredIdea(
        DateTimeOffset weekStart, string name, int upVotes, string? authorId = null)
    {
        BusinessIdeaPost idea = new BusinessIdeaPost
        {
            Name = name,
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = authorId ?? AuthorId,
            // Expiry lands one day into the week.
            CreatedAtUtc = weekStart.AddDays(1 - IdeaRules.LifetimeDays),
        };
        idea.Votes = Enumerable.Range(0, upVotes)
            .Select(i => new PostVote
            {
                PostId = idea.Id,
                UserId = $"voter-{i}",
                Direction = VoteDirection.Up,
            })
            .ToList();
        return idea;
    }

    public static WeeklyWinner GetExistingWinner(DateTimeOffset weekStart)
    {
        return new WeeklyWinner
        {
            PeriodStartUtc = weekStart,
            PeriodEndUtc = weekStart.AddDays(7),
            PostName = "Already declared",
            AuthorId = AuthorId,
        };
    }
}
