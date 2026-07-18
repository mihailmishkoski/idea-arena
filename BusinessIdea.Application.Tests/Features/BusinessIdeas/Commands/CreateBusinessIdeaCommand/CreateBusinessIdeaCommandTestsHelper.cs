#nullable enable
namespace BusinessIdea.Application.Tests.Features.BusinessIdeas.Commands.CreateBusinessIdeaCommandTests;
using BusinessIdea.Application.Features.BusinessIdeas.Commands.CreateBusinessIdea;
using BusinessIdea.Domain.Enums;
using System.Collections.Generic;

public static class CreateBusinessIdeaCommandTestsHelper
{
    public static readonly string UserId = "user-1";
    public static readonly string Name = "  NovaFlow  ";
    public static readonly string UniqueValueProposition = "Zero-setup flow platform.";
    public static readonly string Problem = "Setup takes too long.";
    public static readonly string Solution = "Automate everything.";

    public static CreateBusinessIdeaCommand GetCommand(
        List<BusinessIdeaCategory>? categories = null,
        string? competition = null,
        string? incomeStrategy = null,
        string? exitStrategy = null,
        string? videoPitchUrl = null)
    {
        return new CreateBusinessIdeaCommand
        {
            Name = Name,
            UniqueValueProposition = UniqueValueProposition,
            Problem = Problem,
            Solution = Solution,
            Categories = categories ?? new List<BusinessIdeaCategory> { BusinessIdeaCategory.Tech },
            Competition = competition,
            IncomeStrategy = incomeStrategy,
            ExitStrategy = exitStrategy,
            VideoPitchUrl = videoPitchUrl,
        };
    }
}
