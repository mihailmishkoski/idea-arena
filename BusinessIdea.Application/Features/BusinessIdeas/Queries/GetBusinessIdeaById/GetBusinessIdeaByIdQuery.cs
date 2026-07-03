using MediatR;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeaById;

public record GetBusinessIdeaByIdQuery(Guid Id) : IRequest<BusinessIdeaDetailDto>;
