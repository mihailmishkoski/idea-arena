#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.ChatRequestedEvent;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class ChatRequestedEventTestsHelper
{
    public static readonly string RequesterId = "requester-1";
    public static readonly string RequesterName = "Ava";
    public static readonly string RecipientId = "recipient-1";
    public static readonly string RecipientName = "Bob";
    public static readonly string RecipientEmail = "bob@example.com";
    public static readonly string PostName = "NovaFlow";

    public static PublicAppUrls GetUrls()
    {
        return new PublicAppUrls { BaseUrl = "http://localhost:4200" };
    }

    public static Conversation GetConversation(Guid conversationId, Guid? postId = null)
    {
        return new Conversation
        {
            Id = conversationId,
            RequesterId = RequesterId,
            RecipientId = RecipientId,
            PostId = postId,
            Status = ChatRequestStatus.Pending,
        };
    }

    public static AuthorInfo GetRequester()
    {
        return new AuthorInfo { Id = RequesterId, DisplayName = RequesterName };
    }

    public static AuthorInfo GetRecipient(string? email)
    {
        return new AuthorInfo { Id = RecipientId, DisplayName = RecipientName, Email = email };
    }

    public static BusinessIdeaPost GetIdea(Guid postId)
    {
        return new BusinessIdeaPost
        {
            Id = postId,
            Name = PostName,
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = RecipientId,
        };
    }

    public static OutboxMessage GetMessage(Guid conversationId)
    {
        return OutboxMessageFactory.Create(OutboxEventTypes.ChatRequested, new ChatRequestedPayload(conversationId));
    }
}
