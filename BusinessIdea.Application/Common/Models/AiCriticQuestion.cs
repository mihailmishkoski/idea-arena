using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Application.Common.Models;

/// <summary>One investor-style question aimed at a specific part of a pitch.</summary>
public record AiCriticQuestion(IdeaMetric Metric, string Question);
