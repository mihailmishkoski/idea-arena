using FluentValidation;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeas;

public class GetBusinessIdeasQueryValidator : AbstractValidator<GetBusinessIdeasQuery>
{
    public GetBusinessIdeasQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
