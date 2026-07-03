using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Outbox;

/// <summary>
/// Runs the AI critic against a freshly posted idea and seeds its questions as
/// metric-tagged comments from the system "AI Critic" account, so every idea
/// starts with a discussion instead of dead silence.
/// </summary>
public class IdeaCreatedProcessor : IOutboxProcessor
{
    private readonly IApplicationDbContext _context;
    private readonly IAiCritic _critic;
    private readonly IAiCriticUserProvider _criticUser;

    public IdeaCreatedProcessor(
        IApplicationDbContext context, IAiCritic critic, IAiCriticUserProvider criticUser)
    {
        _context = context;
        _critic = critic;
        _criticUser = criticUser;
    }

    public string Type => OutboxEventTypes.IdeaCreated;

    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = OutboxMessageFactory.Deserialize<IdeaCreatedPayload>(message);

        var idea = await _context.BusinessIdeas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == payload.PostId, cancellationToken);
        if (idea is null)
        {
            return; // Idea deleted before we got to it.
        }

        var criticUserId = await _criticUser.GetAiCriticUserIdAsync(cancellationToken);

        // Idempotency guard: a retry after a partial failure must not
        // double-post the questions.
        var alreadySeeded = await _context.Comments
            .AnyAsync(c => c.PostId == idea.Id && c.AuthorId == criticUserId, cancellationToken);
        if (alreadySeeded)
        {
            return;
        }

        var questions = await _critic.GenerateQuestionsAsync(idea, cancellationToken);

        foreach (var question in questions)
        {
            _context.Comments.Add(new Comment
            {
                PostId = idea.Id,
                AuthorId = criticUserId,
                Content = question.Question,
                TargetMetric = question.Metric,
            });
        }

        if (questions.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
