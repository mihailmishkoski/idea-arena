using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Winners.Queries.GetWinners;

public class GetWinnersQueryHandler : IRequestHandler<GetWinnersQuery, PaginatedList<WeeklyWinnerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWinnersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<WeeklyWinnerDto>> Handle(
        GetWinnersQuery request, CancellationToken cancellationToken)
    {
        var projected = _context.WeeklyWinners
            .AsNoTracking()
            .OrderByDescending(w => w.PeriodStartUtc)
            .Select(w => new WeeklyWinnerDto
            {
                Id = w.Id,
                PeriodStartUtc = w.PeriodStartUtc,
                PeriodEndUtc = w.PeriodEndUtc,
                PostId = w.PostId,
                PostName = w.PostName,
                AuthorId = w.AuthorId,
                AuthorName = _context.Authors
                    .Where(u => u.Id == w.AuthorId)
                    .Select(u => u.DisplayName)
                    .FirstOrDefault(),
                AuthorAvatar = _context.Authors
                    .Where(u => u.Id == w.AuthorId)
                    .Select(u => u.AvatarId)
                    .FirstOrDefault(),
                UpVotes = w.UpVotes,
                DownVotes = w.DownVotes,
                Score = w.Score,
                CommentCount = w.CommentCount,
            });

        return await PaginatedList<WeeklyWinnerDto>.CreateAsync(
            projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
