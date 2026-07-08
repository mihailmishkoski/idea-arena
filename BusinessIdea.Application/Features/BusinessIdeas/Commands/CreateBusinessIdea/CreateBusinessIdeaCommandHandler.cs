using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Domain.Entities;
using MediatR;

namespace BusinessIdea.Application.Features.BusinessIdeas.Commands.CreateBusinessIdea;

public class CreateBusinessIdeaCommandHandler : IRequestHandler<CreateBusinessIdeaCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateBusinessIdeaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateBusinessIdeaCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in to post an idea.");

       var idea = new BusinessIdeaPost
        {
            Name = request.Name.Trim(),
            UniqueValueProposition = request.UniqueValueProposition.Trim(),
            Problem = request.Problem.Trim(),
            Solution = request.Solution.Trim(),
            Competition = request.Competition?.Trim(),
            IncomeStrategy = request.IncomeStrategy?.Trim(),
            ExitStrategy = request.ExitStrategy?.Trim(),
            VideoPitchUrl = request.VideoPitchUrl?.Trim(),
            AuthorId = userId,
            Categories = request.Categories
        };

        _context.BusinessIdeas.Add(idea);

        // Same transaction as the idea itself: the AI critic runs only for
        // ideas that actually got committed (transactional outbox).
        _context.OutboxMessages.Add(OutboxMessageFactory.Create(
            OutboxEventTypes.IdeaCreated, new IdeaCreatedPayload(idea.Id)));

        await _context.SaveChangesAsync(cancellationToken);

        return idea.Id;
    }
}
