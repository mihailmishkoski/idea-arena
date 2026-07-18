using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Enums;
using MediatR;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeas;

public record GetBusinessIdeasQuery : IRequest<PaginatedList<BusinessIdeaSummaryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public IdeaSortOrder SortBy { get; init; } = IdeaSortOrder.Top;
    public string? Search { get; init; }

    /// <summary>Optional category filter — an idea matches if it has ANY of these categories.</summary>
    public List<BusinessIdeaCategory>? Categories { get; init; }
}
