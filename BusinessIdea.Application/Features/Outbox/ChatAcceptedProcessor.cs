using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Outbox;

/// <summary>Emails the requester when their chat request is accepted.</summary>
public class ChatAcceptedProcessor : IOutboxProcessor
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _email;
    private readonly PublicAppUrls _urls;

    public ChatAcceptedProcessor(IApplicationDbContext context, IEmailSender email, PublicAppUrls urls)
    {
        _context = context;
        _email = email;
        _urls = urls;
    }

    public string Type => OutboxEventTypes.ChatAccepted;

    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = OutboxMessageFactory.Deserialize<ChatAcceptedPayload>(message);

        var conversation = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == payload.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return;
        }

        var recipient = await _context.Authors
            .Where(a => a.Id == conversation.RecipientId)
            .FirstOrDefaultAsync(cancellationToken);
        var requester = await _context.Authors
            .Where(a => a.Id == conversation.RequesterId)
            .FirstOrDefaultAsync(cancellationToken);
        if (requester?.Email is null)
        {
            return;
        }

        var accepterName = recipient?.DisplayName ?? "Your contact";

        var body = ChatRequestedProcessor.EmailBody(
            $"<strong>{accepterName}</strong> accepted your request — the conversation is open!",
            "Jump back in and send the first message.",
            _urls.Messages(), "Open the conversation");

        await _email.SendAsync(
            new EmailMessage(requester.Email, requester.DisplayName,
                $"{accepterName} accepted your request on Idea Arena", body),
            cancellationToken);
    }
}
