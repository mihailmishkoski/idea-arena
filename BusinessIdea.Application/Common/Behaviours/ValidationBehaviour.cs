using FluentValidation;
using MediatR;
using ValidationException = BusinessIdea.Application.Common.Exceptions.ValidationException;

namespace BusinessIdea.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that runs every registered <see cref="IValidator{T}"/>
/// for a request before its handler executes. This keeps validation as a single,
/// cross-cutting concern rather than repeating guard clauses in every handler
/// (Single Responsibility / Open-Closed).
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
