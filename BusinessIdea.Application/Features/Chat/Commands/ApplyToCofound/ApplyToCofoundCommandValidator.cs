using FluentValidation;

namespace BusinessIdea.Application.Features.Chat.Commands.ApplyToCofound;

public class ApplyToCofoundCommandValidator : AbstractValidator<ApplyToCofoundCommand>
{
    public ApplyToCofoundCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Role).MaximumLength(100);
        RuleFor(x => x.Skills).MaximumLength(500);
        RuleFor(x => x.Motivation).MaximumLength(1000);
        RuleFor(x => x.Availability).MaximumLength(100);
        RuleFor(x => x.ContactLink).MaximumLength(300);
    }
}
