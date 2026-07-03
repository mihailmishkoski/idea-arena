using MediatR;

namespace BusinessIdea.Application.Features.BusinessIdeas.Commands.UpdateBusinessIdea;

/// <summary>Updates an existing business idea. Only the author may do this.</summary>
public record UpdateBusinessIdeaCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UniqueValueProposition { get; init; } = string.Empty;
    public string Problem { get; init; } = string.Empty;
    public string Solution { get; init; } = string.Empty;
    public string? Competition { get; init; }
    public string? IncomeStrategy { get; init; }
    public string? ExitStrategy { get; init; }
    public string? VideoPitchUrl { get; init; }
}
