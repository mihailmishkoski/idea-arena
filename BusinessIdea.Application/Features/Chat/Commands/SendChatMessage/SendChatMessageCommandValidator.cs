using FluentValidation;

namespace BusinessIdea.Application.Features.Chat.Commands.SendChatMessage;

public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("A message cannot be empty.")
            .MaximumLength(2000);
    }
}
