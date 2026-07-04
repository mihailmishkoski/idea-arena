using BusinessIdea.Application.Common.Models;
using MediatR;

namespace BusinessIdea.Application.Features.Winners.Queries.GetWinners;

/// <summary>Pages through past weekly winners, most recent week first.</summary>
public record GetWinnersQuery : IRequest<PaginatedList<WeeklyWinnerDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
