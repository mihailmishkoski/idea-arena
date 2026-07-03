using FluentValidation;

namespace BusinessIdea.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        // Top-level comments must name a post; replies derive it from the parent.
        RuleFor(x => x.PostId)
            .NotEmpty().When(x => x.ParentCommentId is null)
            .WithMessage("A post is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("A comment cannot be empty.")
            .MaximumLength(5000);

        RuleFor(x => x.TargetMetric)
            .IsInEnum().WithMessage("Unknown comment target.");
    }
}
