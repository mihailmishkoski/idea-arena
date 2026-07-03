using MediatR;

namespace BusinessIdea.Application.Features.BusinessIdeas.Commands.CreateBusinessIdea;

/// <summary>
/// Creates a new business idea authored by the current user. Returns the new id.
/// </summary>
public record CreateBusinessIdeaCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string UniqueValueProposition { get; init; } = string.Empty;
    public string Problem { get; init; } = string.Empty;
    public string Solution { get; init; } = string.Empty;
    public string? Competition { get; init; }
    public string? IncomeStrategy { get; init; }
    public string? ExitStrategy { get; init; }
    public string? VideoPitchUrl { get; init; }
}
