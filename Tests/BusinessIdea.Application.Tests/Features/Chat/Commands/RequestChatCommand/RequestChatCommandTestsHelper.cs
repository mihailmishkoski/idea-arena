#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.RequestChatCommandTests;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

public static class RequestChatCommandTestsHelper
{
    public static readonly string RequesterId = "requester-1";
    public static readonly string RequesterName = "Ava";
    public static readonly string RecipientId = "recipient-1";

    public static AuthorInfo GetRequester()
    {
        return new AuthorInfo { Id = RequesterId, DisplayName = RequesterName };
    }

    public static AuthorInfo GetRecipient()
    {
        return new AuthorInfo { Id = RecipientId, DisplayName = "Bob" };
    }

    public static Conversation GetConversation(ChatRequestStatus status)
    {
        return new Conversation
        {
            RequesterId = RequesterId,
            RecipientId = RecipientId,
            Status = status,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Enough fresh conversations to other users to hit the daily cap.</summary>
    public static List<Conversation> GetConversationsAtDailyLimit()
    {
        return Enumerable.Range(0, ChatRules.MaxInitiationsPerDay)
            .Select(i => new Conversation
            {
                RequesterId = RequesterId,
                RecipientId = $"someone-else-{i}",
                Status = ChatRequestStatus.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            })
            .ToList();
    }
}
