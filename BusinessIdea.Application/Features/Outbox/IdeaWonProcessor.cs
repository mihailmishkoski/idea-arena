using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Outbox;

/// <summary>Emails the author whose idea won its competition week.</summary>
public class IdeaWonProcessor : IOutboxProcessor
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _email;
    private readonly PublicAppUrls _urls;

    public IdeaWonProcessor(IApplicationDbContext context, IEmailSender email, PublicAppUrls urls)
    {
        _context = context;
        _email = email;
        _urls = urls;
    }

    public string Type => OutboxEventTypes.IdeaWon;

    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = OutboxMessageFactory.Deserialize<IdeaWonPayload>(message);

        var winner = await _context.WeeklyWinners
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == payload.WinnerId, cancellationToken);
        if (winner is null)
        {
            return;
        }

        var author = await _context.Authors
            .Where(a => a.Id == winner.AuthorId)
            .FirstOrDefaultAsync(cancellationToken);
        if (author?.Email is null)
        {
            return;
        }

        var link = winner.PostId is { } postId ? _urls.Idea(postId) : _urls.BaseUrl;

        var body = ChatRequestedProcessor.EmailBody(
            $"Congratulations — your idea <strong>“{winner.PostName}”</strong> won its competition week!",
            $"It finished with a score of <strong>{winner.Score}</strong> " +
            $"({winner.UpVotes} up / {winner.DownVotes} down) and {winner.CommentCount} comments. " +
            "It now has a permanent place in the Hall of Fame.",
            link, "See your winning idea");

        await _email.SendAsync(
            new EmailMessage(author.Email, author.DisplayName,
                $"“{winner.PostName}” won on Idea Arena!", body),
            cancellationToken);
    }
}
