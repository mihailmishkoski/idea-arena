using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Outbox;

/// <summary>
/// Emails the recipient of a new chat / co-founder request. Data is re-read
/// fresh here (not carried in the payload) so the email never shows stale
/// names or a deleted conversation.
/// </summary>
public class ChatRequestedProcessor : IOutboxProcessor
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _email;
    private readonly PublicAppUrls _urls;

    public ChatRequestedProcessor(IApplicationDbContext context, IEmailSender email, PublicAppUrls urls)
    {
        _context = context;
        _email = email;
        _urls = urls;
    }

    public string Type => OutboxEventTypes.ChatRequested;

    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = OutboxMessageFactory.Deserialize<ChatRequestedPayload>(message);

        var conversation = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == payload.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return; // Conversation is gone — nothing to announce.
        }

        var requester = await LoadAuthorAsync(conversation.RequesterId, cancellationToken);
        var recipient = await LoadAuthorAsync(conversation.RecipientId, cancellationToken);
        if (recipient?.Email is null)
        {
            return; // No address to deliver to; not an error worth retrying.
        }

        var requesterName = requester?.DisplayName ?? "Someone";

        string? postName = null;
        if (conversation.PostId is { } postId)
        {
            postName = await _context.BusinessIdeas
                .Where(p => p.Id == postId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var (subject, intro, link) = postName is null
            ? ($"{requesterName} wants to chat with you on Idea Arena",
               $"<strong>{requesterName}</strong> sent you a chat request.",
               _urls.Messages())
            : ($"{requesterName} wants to join “{postName}”",
               $"<strong>{requesterName}</strong> is interested in co-founding your idea <strong>“{postName}”</strong> and sent you a request.",
               conversation.PostId is { } pid ? _urls.Idea(pid) : _urls.Messages());

        var body = EmailBody(intro,
            "Accept the request to open a private conversation, or decline it — your call.",
            link, "Open Idea Arena");

        await _email.SendAsync(
            new EmailMessage(recipient.Email, recipient.DisplayName, subject, body),
            cancellationToken);
    }

    private Task<AuthorInfo?> LoadAuthorAsync(string id, CancellationToken ct) =>
        _context.Authors.Where(a => a.Id == id).FirstOrDefaultAsync(ct);

    internal static string EmailBody(string intro, string detail, string link, string linkText) => $"""
        <div style="font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:0 auto;border:1px solid #ccc;border-radius:6px;overflow:hidden">
          <div style="background:#ff4500;color:#fff;padding:14px 20px;font-weight:700;font-size:18px">&#9650; Idea Arena</div>
          <div style="padding:20px;color:#1c1c1c;font-size:15px;line-height:1.6">
            <p>{intro}</p>
            <p>{detail}</p>
            <p style="margin-top:24px">
              <a href="{link}" style="background:#ff4500;color:#fff;text-decoration:none;padding:10px 22px;border-radius:999px;font-weight:700">{linkText}</a>
            </p>
          </div>
          <div style="padding:12px 20px;color:#7c7c7c;font-size:12px;border-top:1px solid #edeff1">
            You received this because you have an Idea Arena account.
          </div>
        </div>
        """;
}
