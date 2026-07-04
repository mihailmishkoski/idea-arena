#nullable enable
namespace BusinessIdea.Application.Tests.Features.Chat.Commands.ApplyToCofoundCommandTests;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Features.Chat.Commands.ApplyToCofound;
using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ApplyToCofoundCommandTestsHelper
{
    public static readonly string ApplicantId = "applicant-1";
    public static readonly string ApplicantName = "Ava";
    public static readonly string AuthorId = "idea-author";
    public static readonly string Role = "CTO — I'd own the tech";
    public static readonly string PostName = "NovaFlow";

    public static BusinessIdeaPost GetIdea(Guid postId)
    {
        return new BusinessIdeaPost
        {
            Id = postId,
            Name = PostName,
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = AuthorId,
        };
    }

    public static AuthorInfo GetApplicant()
    {
        return new AuthorInfo { Id = ApplicantId, DisplayName = ApplicantName };
    }

    public static Conversation GetConversation(ChatRequestStatus status)
    {
        return new Conversation
        {
            RequesterId = ApplicantId,
            RecipientId = AuthorId,
            Status = status,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static List<Conversation> GetConversationsAtDailyLimit()
    {
        return Enumerable.Range(0, ChatRules.MaxInitiationsPerDay)
            .Select(i => new Conversation
            {
                RequesterId = ApplicantId,
                RecipientId = $"someone-else-{i}",
                Status = ChatRequestStatus.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            })
            .ToList();
    }

    public static ApplyToCofoundCommand GetCommand(Guid postId)
    {
        return new ApplyToCofoundCommand(postId, Role, null, null, null, null);
    }
}
