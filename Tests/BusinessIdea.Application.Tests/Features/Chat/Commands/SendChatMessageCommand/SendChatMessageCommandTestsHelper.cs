#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.SendChatMessageCommandTests;

using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class SendChatMessageCommandTestsHelper
{
    public static readonly string RequesterId = "requester-1";
    public static readonly string RecipientId = "recipient-1";
    public static readonly string OutsiderId = "outsider";
    public static readonly string Content = "  Hello there!  ";

    public static Conversation GetConversation(Guid conversationId, ChatRequestStatus status = ChatRequestStatus.Accepted)
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
}
