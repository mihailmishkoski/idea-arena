using MediatR;

namespace BusinessIdea.Application.Features.BusinessIdeas.Commands.DeleteBusinessIdea;

/// <summary>Deletes a business idea. Only the author may do this.</summary>
public record DeleteBusinessIdeaCommand(Guid Id) : IRequest;
