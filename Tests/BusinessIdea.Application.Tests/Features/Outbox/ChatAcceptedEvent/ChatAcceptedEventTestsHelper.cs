#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.ChatAcceptedEvent;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class ChatAcceptedEventTestsHelper
{
    public static readonly string RequesterId = "requester-1";
    public static readonly string RequesterEmail = "ava@example.com";
    public static readonly string RecipientId = "recipient-1";
    public static readonly string RecipientName = "Bob";

    public static PublicAppUrls GetUrls()
    {
        return new PublicAppUrls { BaseUrl = "http://localhost:4200" };
    }

    public static Conversation GetConversation(Guid conversationId)
    {
        return new Conversation
        {
            Id = conversationId,
            RequesterId = RequesterId,
            RecipientId = RecipientId,
            Status = ChatRequestStatus.Accepted,
        };
    }

    public static AuthorInfo GetRequester(string? email)
    {
        return new AuthorInfo { Id = RequesterId, DisplayName = "Ava", Email = email };
    }

    public static AuthorInfo GetRecipient()
    {
        return new AuthorInfo { Id = RecipientId, DisplayName = RecipientName };
    }

    public static OutboxMessage GetMessage(Guid conversationId)
    {
        return OutboxMessageFactory.Create(OutboxEventTypes.ChatAccepted, new ChatAcceptedPayload(conversationId));
    }
}
