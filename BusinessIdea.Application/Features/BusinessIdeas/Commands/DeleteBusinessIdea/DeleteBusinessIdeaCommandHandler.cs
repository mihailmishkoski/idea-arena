using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.BusinessIdeas.Commands.DeleteBusinessIdea;

public class DeleteBusinessIdeaCommandHandler : IRequestHandler<DeleteBusinessIdeaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteBusinessIdeaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteBusinessIdeaCommand request, CancellationToken cancellationToken)
    {
        var idea = await _context.BusinessIdeas
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.BusinessIdeaPost), request.Id);

        if (idea.AuthorId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You can only delete your own ideas.");
        }

        idea.DeletedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
