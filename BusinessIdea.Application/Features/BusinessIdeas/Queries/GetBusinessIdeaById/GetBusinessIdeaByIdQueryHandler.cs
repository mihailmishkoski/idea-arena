using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeaById;

public class GetBusinessIdeaByIdQueryHandler : IRequestHandler<GetBusinessIdeaByIdQuery, BusinessIdeaDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBusinessIdeaByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BusinessIdeaDetailDto> Handle(GetBusinessIdeaByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var dto = await _context.BusinessIdeas
            .AsNoTracking()
            .Where(i => i.Id == request.Id)
            .Select(i => new BusinessIdeaDetailDto
            {
                Id = i.Id,
                Name = i.Name,
                UniqueValueProposition = i.UniqueValueProposition,
                Problem = i.Problem,
                Solution = i.Solution,
                Competition = i.Competition,
                IncomeStrategy = i.IncomeStrategy,
                ExitStrategy = i.ExitStrategy,
                VideoPitchUrl = i.VideoPitchUrl,
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
                UpdatedAtUtc = i.UpdatedAtUtc,
                UpVotes = i.Votes.Count(v => v.Direction == VoteDirection.Up),
                DownVotes = i.Votes.Count(v => v.Direction == VoteDirection.Down),
                CommentCount = i.Comments.Count,
                CurrentUserVote = userId == null
                    ? null
                    : i.Votes
                        .Where(v => v.UserId == userId)
                        .Select(v => (VoteDirection?)v.Direction)
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.BusinessIdeaPost), request.Id);

        return dto;
    }
}
