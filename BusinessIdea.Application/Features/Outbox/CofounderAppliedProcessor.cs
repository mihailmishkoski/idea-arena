using System.Net;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Outbox;

/// <summary>
/// Emails the idea's author the full co-founder application — role, skills,
/// motivation — so they can size up the applicant straight from their inbox.
/// </summary>
public class CofounderAppliedProcessor : IOutboxProcessor
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _email;
    private readonly PublicAppUrls _urls;

    public CofounderAppliedProcessor(IApplicationDbContext context, IEmailSender email, PublicAppUrls urls)
    {
        _context = context;
        _email = email;
        _urls = urls;
    }

    public string Type => OutboxEventTypes.CofounderApplied;

    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = OutboxMessageFactory.Deserialize<CofounderAppliedPayload>(message);

        var conversation = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == payload.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return;
        }

        var applicant = await _context.Authors
            .Where(a => a.Id == conversation.RequesterId)
            .FirstOrDefaultAsync(cancellationToken);
        var author = await _context.Authors
            .Where(a => a.Id == conversation.RecipientId)
            .FirstOrDefaultAsync(cancellationToken);
        if (author?.Email is null)
        {
            return;
        }

        string? postName = null;
        if (conversation.PostId is { } postId)
        {
            postName = await _context.BusinessIdeas
                .Where(p => p.Id == postId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var applicantName = applicant?.DisplayName ?? "Someone";
        var subject = postName is null
            ? $"{applicantName} applied to co-found with you"
            : $"{applicantName} applied to co-found “{postName}”";

        // The application text is plain text from the user — escape it, then
        // preserve the line structure.
        var applicationHtml = WebUtility.HtmlEncode(payload.ApplicationText).Replace("\n", "<br>");

        var body = ChatRequestedProcessor.EmailBody(
            $"<strong>{WebUtility.HtmlEncode(applicantName)}</strong> wants to build with you. Their application:",
            $"<div style=\"background:#f6f7f8;border:1px solid #edeff1;border-radius:6px;padding:14px 16px;margin-top:8px\">{applicationHtml}</div>",
            conversation.PostId is { } pid ? _urls.Idea(pid) : _urls.Messages(),
            "Review & respond");

        await _email.SendAsync(
            new EmailMessage(author.Email, author.DisplayName, subject, body),
            cancellationToken);
    }
}
