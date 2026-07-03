using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Web.Models;

/// <summary>Body for a vote request; the target id comes from the route.</summary>
public record VoteRequest(VoteDirection Direction);
