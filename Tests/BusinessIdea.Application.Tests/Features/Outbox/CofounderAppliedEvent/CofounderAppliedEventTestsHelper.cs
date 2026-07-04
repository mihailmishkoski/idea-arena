#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.CofounderAppliedEvent;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;

public static class CofounderAppliedEventTestsHelper
{
    public static readonly string ApplicantId = "applicant-1";
    public static readonly string ApplicantName = "Ava";
    public static readonly string AuthorId = "idea-author";
    public static readonly string AuthorEmail = "author@example.com";
    public static readonly string ApplicationText = "Role I can take: CTO <script>alert(1)</script>";

    public static PublicAppUrls GetUrls()
    {
        return new PublicAppUrls { BaseUrl = "http://localhost:4200" };
    }

    public static Conversation GetConversation(Guid conversationId, Guid? postId = null)
    {
        return new Conversation
        {
            Id = conversationId,
            RequesterId = ApplicantId,
            RecipientId = AuthorId,
            PostId = postId,
            Status = ChatRequestStatus.Pending,
        };
    }

    public static AuthorInfo GetApplicant()
    {
        return new AuthorInfo { Id = ApplicantId, DisplayName = ApplicantName };
    }

    public static AuthorInfo GetAuthor(string? email)
    {
        return new AuthorInfo { Id = AuthorId, DisplayName = "Bob", Email = email };
    }

    public static OutboxMessage GetMessage(Guid conversationId)
    {
        return OutboxMessageFactory.Create(
            OutboxEventTypes.CofounderApplied,
            new CofounderAppliedPayload(conversationId, ApplicationText));
    }
}
