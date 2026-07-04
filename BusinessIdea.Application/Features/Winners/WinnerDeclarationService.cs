using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Winners;

/// <summary>
/// A competition week runs Monday 00:00 UTC → next Monday 00:00 UTC. An idea
/// belongs to the week its <see cref="IdeaRules.LifetimeDays"/>-day window
/// closes in; the idea with the highest net score of that week wins. The winner
/// is snapshotted into <see cref="WeeklyWinner"/>, and for freshly finished
/// weeks the author gets a notification plus an "idea-won" outbox email.
/// Backfilled ancient weeks (seed data, long downtime) get rows only — no one
/// wants a congratulation email for something that happened two months ago.
/// </summary>
public class WinnerDeclarationService : IWinnerDeclarationService
{
    /// <summary>How recently a week must have ended for the author to be congratulated.</summary>
    private static readonly TimeSpan CongratulationWindow = TimeSpan.FromDays(7);

    private readonly IApplicationDbContext _context;

    public WinnerDeclarationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> DeclareDueWinnersAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var currentWeekStart = WeekStartUtc(now);

        // The first week that could possibly have a winner is the week the
        // earliest idea's window closed in.
        var earliestCreated = await _context.BusinessIdeas
            .OrderBy(i => i.CreatedAtUtc)
            .Select(i => (DateTimeOffset?)i.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (earliestCreated is null)
        {
            return 0;
        }

        var week = WeekStartUtc(earliestCreated.Value.AddDays(IdeaRules.LifetimeDays));

        var declaredWeeks = await _context.WeeklyWinners
            .Select(w => w.PeriodStartUtc)
            .ToListAsync(cancellationToken);
        var declared = declaredWeeks.ToHashSet();

        var newWinners = 0;

        for (; week < currentWeekStart; week = week.AddDays(7))
        {
            if (declared.Contains(week))
            {
                continue;
            }

            var weekEnd = week.AddDays(7);

            // Ideas whose active window closed inside [week, weekEnd).
            var windowOpenedFrom = week.AddDays(-IdeaRules.LifetimeDays);
            var windowOpenedTo = weekEnd.AddDays(-IdeaRules.LifetimeDays);

            var top = await _context.BusinessIdeas
                .AsNoTracking()
                .Where(i => i.CreatedAtUtc >= windowOpenedFrom && i.CreatedAtUtc < windowOpenedTo)
                .OrderByDescending(i =>
                    i.Votes.Count(v => v.Direction == VoteDirection.Up) -
                    i.Votes.Count(v => v.Direction == VoteDirection.Down))
                .ThenByDescending(i => i.Comments.Count)
                .ThenBy(i => i.CreatedAtUtc)
                .Select(i => new
                {
                    i.Id,
                    i.Name,
                    i.AuthorId,
                    UpVotes = i.Votes.Count(v => v.Direction == VoteDirection.Up),
                    DownVotes = i.Votes.Count(v => v.Direction == VoteDirection.Down),
                    CommentCount = i.Comments.Count,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (top is null)
            {
                continue; // Nothing competed that week — no winner row.
            }

            var winner = new WeeklyWinner
            {
                PeriodStartUtc = week,
                PeriodEndUtc = weekEnd,
                PostId = top.Id,
                PostName = top.Name,
                AuthorId = top.AuthorId,
                UpVotes = top.UpVotes,
                DownVotes = top.DownVotes,
                CommentCount = top.CommentCount,
                Score = top.UpVotes - top.DownVotes,
            };
            _context.WeeklyWinners.Add(winner);

            // Congratulate only for freshly finished weeks; backfills stay silent.
            if (now - weekEnd <= CongratulationWindow)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = top.AuthorId,
                    Type = NotificationType.IdeaWon,
                    Text = $"Your idea “{top.Name}” won its competition week!",
                    TargetId = top.Id,
                });

                _context.OutboxMessages.Add(OutboxMessageFactory.Create(
                    OutboxEventTypes.IdeaWon, new IdeaWonPayload(winner.Id)));
            }

            newWinners++;
        }

        if (newWinners > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return newWinners;
    }

    /// <summary>Monday 00:00 UTC of the week containing <paramref name="moment"/>.</summary>
    public static DateTimeOffset WeekStartUtc(DateTimeOffset moment)
    {
        var date = moment.UtcDateTime.Date;
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return new DateTimeOffset(date.AddDays(-daysSinceMonday), TimeSpan.Zero);
    }
}
