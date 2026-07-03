using BusinessIdea.Application.Common.Models;
using MediatR;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeas;

/// <summary>A paged, sortable feed of business ideas.</summary>
public record GetBusinessIdeasQuery : IRequest<PaginatedList<BusinessIdeaSummaryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public IdeaSortOrder SortBy { get; init; } = IdeaSortOrder.Top;

    /// <summary>Optional free-text search over name, value proposition and problem.</summary>
    public string? Search { get; init; }
}
