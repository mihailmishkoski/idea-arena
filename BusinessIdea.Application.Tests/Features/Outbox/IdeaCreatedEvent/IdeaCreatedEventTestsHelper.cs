#nullable enable
namespace BusinessIdea.Application.Tests.Features.Outbox.IdeaCreatedEvent;

using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using System;
using System.Collections.Generic;

public static class IdeaCreatedEventTestsHelper
{
    public static readonly string CriticUserId = "ai-critic";
    public static readonly string AuthorId = "idea-author";

    public static BusinessIdeaPost GetIdea(Guid postId)
    {
        return new BusinessIdeaPost
        {
            Id = postId,
            Name = "NovaFlow",
            UniqueValueProposition = "UVP",
            Problem = "Problem",
            Solution = "Solution",
            AuthorId = AuthorId,
        };
    }

    public static OutboxMessage GetMessage(Guid postId)
    {
        return OutboxMessageFactory.Create(OutboxEventTypes.IdeaCreated, new IdeaCreatedPayload(postId));
    }

    public static IReadOnlyList<AiCriticQuestion> GetQuestions()
    {
        return new List<AiCriticQuestion>
        {
            new AiCriticQuestion(IdeaMetric.Problem, "Is the problem painful enough?"),
            new AiCriticQuestion(IdeaMetric.Competition, "Who else is doing this?"),
            new AiCriticQuestion(IdeaMetric.IncomeStrategy, "Who pays, and how much?"),
        };
    }
}
