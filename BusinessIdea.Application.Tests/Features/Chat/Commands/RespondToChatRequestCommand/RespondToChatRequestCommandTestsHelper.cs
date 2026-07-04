#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.RespondToChatRequestCommandTests;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class RespondToChatRequestCommandTestsHelper
{
    public static readonly string RequesterId = "requester-1";
    public static readonly string RecipientId = "recipient-1";
    public static readonly string RecipientName = "Bob";

    public static Conversation GetConversation(Guid conversationId, ChatRequestStatus status = ChatRequestStatus.Pending)
    {
        return new Conversation
        {
            Id = conversationId,
            RequesterId = RequesterId,
            RecipientId = RecipientId,
            Status = status,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static AuthorInfo GetRecipient()
    {
        return new AuthorInfo { Id = RecipientId, DisplayName = RecipientName };
    }
}
