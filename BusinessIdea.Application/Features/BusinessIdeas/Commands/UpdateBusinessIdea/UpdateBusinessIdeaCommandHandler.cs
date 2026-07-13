using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.BusinessIdeas.Commands.UpdateBusinessIdea;

public class UpdateBusinessIdeaCommandHandler : IRequestHandler<UpdateBusinessIdeaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateBusinessIdeaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateBusinessIdeaCommand request, CancellationToken cancellationToken)
    {
        var idea = await _context.BusinessIdeas
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.BusinessIdeaPost), request.Id);

        if (idea.AuthorId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You can only edit your own ideas.");
        }
          if (DateTime.UtcNow - idea.CreatedAtUtc <= TimeSpan.FromMinutes(30))
        {
            throw new ForbiddenAccessException("Business ideas can only be edited within 30 minutes of creation.");
        }
       

        idea.Name = request.Name.Trim();
        idea.UniqueValueProposition = request.UniqueValueProposition.Trim();
        idea.Problem = request.Problem.Trim();
        idea.Solution = request.Solution.Trim();
        idea.Competition = request.Competition?.Trim();
        idea.IncomeStrategy = request.IncomeStrategy?.Trim();
        idea.ExitStrategy = request.ExitStrategy?.Trim();
        idea.VideoPitchUrl = request.VideoPitchUrl?.Trim();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
