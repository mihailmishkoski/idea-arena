using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeas;

public class GetBusinessIdeasQueryHandler : IRequestHandler<GetBusinessIdeasQuery, PaginatedList<BusinessIdeaSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBusinessIdeasQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<BusinessIdeaSummaryDto>> Handle(
        GetBusinessIdeasQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddDays(-IdeaRules.LifetimeDays);

        IQueryable<BusinessIdeaPost> query = _context.BusinessIdeas.AsNoTracking();

        // Free-text search (case-insensitive) over the headline fields. Uses the
        // provider-agnostic Like with ToLower so the Application layer stays
        // independent of PostgreSQL's ILike.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim().ToLower()}%";
            query = query.Where(i =>
                EF.Functions.Like(i.Name.ToLower(), term) ||
                EF.Functions.Like(i.UniqueValueProposition.ToLower(), term) ||
                EF.Functions.Like(i.Problem.ToLower(), term));
        }

        // Category filter: an idea matches if it has at least one category
        // in common with the requested set. Empty/null means no filtering.
        if (request.Categories is { Count: > 0 })
        {
            var categoryFilter = request.Categories.ToArray();
            query = query.Where(i => i.Categories.Any(c => categoryFilter.Contains(c)));
        }

        // "Winners" shows closed ideas; every other sort shows only active ones.
        query = request.SortBy == IdeaSortOrder.Winners
            ? query.Where(i => i.CreatedAtUtc <= cutoff)
            : query.Where(i => i.CreatedAtUtc > cutoff);

        query = request.SortBy switch
        {
            IdeaSortOrder.New => query.OrderByDescending(i => i.CreatedAtUtc),
            IdeaSortOrder.Old => query.OrderBy(i => i.CreatedAtUtc),
            IdeaSortOrder.Best => query
                .OrderByDescending(i => i.Votes.Count + i.Comments.Count)
                .ThenByDescending(i => i.CreatedAtUtc),
            IdeaSortOrder.Controversial => query
                .OrderByDescending(i => i.Votes.Count(v => v.Direction == VoteDirection.Down))
                .ThenByDescending(i => i.Votes.Count),
            _ => query // Top and Winners: rank by net score.
                .OrderByDescending(i =>
                    i.Votes.Count(v => v.Direction == VoteDirection.Up) -
                    i.Votes.Count(v => v.Direction == VoteDirection.Down))
                .ThenByDescending(i => i.CreatedAtUtc)
        };

        var projected = query.Select(i => new BusinessIdeaSummaryDto
        {
            Id = i.Id,
            Name = i.Name,
            UniqueValueProposition = i.UniqueValueProposition,
            Categories = i.Categories,
            AuthorId = i.AuthorId,
            AuthorName = _context.Authors
                .Where(u => u.Id == i.AuthorId)
                .Select(u => u.DisplayName)
                .FirstOrDefault(),
            AuthorAvatar = _context.Authors
                .Where(u => u.Id == i.AuthorId)
                .Select(u => u.AvatarId)
                .FirstOrDefault(),
            CreatedAtUtc = i.CreatedAtUtc,
            UpVotes = i.Votes.Count(v => v.Direction == VoteDirection.Up),
            DownVotes = i.Votes.Count(v => v.Direction == VoteDirection.Down),
            CommentCount = i.Comments.Count,
            CurrentUserVote = userId == null
                ? null
                : i.Votes
                    .Where(v => v.UserId == userId)
                    .Select(v => (VoteDirection?)v.Direction)
                    .FirstOrDefault()
        });

        return await PaginatedList<BusinessIdeaSummaryDto>.CreateAsync(
            projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

