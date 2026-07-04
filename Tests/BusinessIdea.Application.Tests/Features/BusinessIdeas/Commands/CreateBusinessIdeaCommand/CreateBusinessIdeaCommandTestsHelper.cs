#nullable enable
namespace BusinessIdea.Application.Tests.Features.BusinessIdeas.Commands.CreateBusinessIdeaCommandTests;

using BusinessIdea.Application.Features.BusinessIdeas.Commands.CreateBusinessIdea;

public static class CreateBusinessIdeaCommandTestsHelper
{
    public static readonly string UserId = "user-1";
    public static readonly string Name = "  NovaFlow  ";
    public static readonly string UniqueValueProposition = "Zero-setup flow platform.";
    public static readonly string Problem = "Setup takes too long.";
    public static readonly string Solution = "Automate everything.";

    public static CreateBusinessIdeaCommand GetCommand()
    {
        return new CreateBusinessIdeaCommand
        {
            Name = Name,
            UniqueValueProposition = UniqueValueProposition,
            Problem = Problem,
            Solution = Solution,
        };
    }
}
