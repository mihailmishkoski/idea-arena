using FluentValidation;

namespace BusinessIdea.Application.Features.BusinessIdeas.Commands.UpdateBusinessIdea;

public class UpdateBusinessIdeaCommandValidator : AbstractValidator<UpdateBusinessIdeaCommand>
{
    public UpdateBusinessIdeaCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A business name is required.")
            .MaximumLength(150);

        RuleFor(x => x.UniqueValueProposition)
            .NotEmpty().WithMessage("A unique value proposition is required.")
            .MaximumLength(1000);

        RuleFor(x => x.Problem)
            .NotEmpty().WithMessage("Describe the problem your idea solves.")
            .MaximumLength(2000);

        RuleFor(x => x.Solution)
            .NotEmpty().WithMessage("Describe your solution.")
            .MaximumLength(2000);

        RuleFor(x => x.Competition).MaximumLength(2000);
        RuleFor(x => x.IncomeStrategy).MaximumLength(2000);
        RuleFor(x => x.ExitStrategy).MaximumLength(2000);

        RuleFor(x => x.VideoPitchUrl)
            .MaximumLength(2048)
            .Must(BeAValidUrl).WithMessage("The video pitch must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.VideoPitchUrl));
    }

    private static bool BeAValidUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var result)
        && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
}
